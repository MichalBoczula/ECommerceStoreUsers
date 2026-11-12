"""Publish evidence produced by the required Users acceptance suite."""

import argparse
import hashlib
import json
import os
from pathlib import Path
from urllib.error import HTTPError
from urllib.parse import urlencode, urlsplit
from urllib.request import HTTPRedirectHandler, Request, build_opener

ROOT = Path(__file__).resolve().parents[1]
REPOSITORY = "https://github.com/MichalBoczula/ECommerceStoreUsers"
ACCEPTANCE = ROOT / "tests/ECommerceStoreUsers.AcceptanceTests"
CHECK_IDS = {"web": "web-users-profile-lookup", "bff": "bff-users-registration"}


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("consumer", choices=CHECK_IDS)
    parser.add_argument("report", type=Path)
    parser.add_argument("receipt", type=Path)
    args = parser.parse_args()
    ref = os.environ["GITHUB_REF"]
    if os.environ["GITHUB_EVENT_NAME"] not in ("push", "workflow_dispatch") or ref != "refs/heads/master":
        raise ValueError("Users evidence requires a push or manual run on master.")

    url = urlsplit(os.environ.get("PACT_REGISTRY_URL", ""))
    token = os.environ.get("PACT_PUBLISHER_TOKEN", "")
    if (
        url.scheme != "https" or not url.hostname or url.username is not None
        or url.password is not None or url.path not in ("", "/") or url.query or url.fragment or not token
    ):
        raise ValueError("Configure an HTTPS PACT_REGISTRY_URL origin and the Users PACT_PUBLISHER_TOKEN.")
    opener = build_opener(NoRedirect())

    def call(path, body=None, raw=False):
        headers = {} if body is None else {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
        request = Request(f"https://{url.netloc}{path}", data=None if body is None else json.dumps(body).encode(), headers=headers)
        try:
            with opener.open(request, timeout=90) as response:
                return response.read() if raw else json.load(response)
        except HTTPError as error:
            raise RuntimeError(f"Pact registry rejected {path.partition('?')[0]} with HTTP {error.code}.") from None

    consumer = args.consumer
    source = json.loads((ROOT / f"contracts/pact/{consumer}-source.json").read_bytes())
    report = json.loads(args.report.read_bytes())
    sha = os.environ["GITHUB_SHA"]
    digest = source["contractSha256"]
    if (
        report["consumerCommitSha"] != source["commitSha"] or report["providerCommitSha"] != sha
        or report["contractSha256"] != digest or report["status"] != "Passed"
        or report["verifiedInteractions"] != source["interactionCount"] or report["failedInteractions"] != 0
    ):
        raise ValueError("The report must verify all interactions for the pinned Pact and this Users commit.")

    filter_by = {
        "sourceApplicationId": consumer, "consumerApplicationId": consumer,
        "providerApplicationId": "users", "kind": "ConsumerPact", "commitSha": source["commitSha"],
        "contractSha256": digest, "sourceRef": "refs/heads/master", "limit": 100,
    }
    consumer_build = f"https://github.com/{source['repository']}/actions/runs/{source['workflowRunId']}"
    matches = []
    seen = set()
    while True:
        page = call("/api/contract-publications?" + urlencode(filter_by))
        matches.extend(item for item in page["items"] if item["buildUrl"] == consumer_build)
        after = page["nextCursor"]
        if after is None:
            break
        if after in seen:
            raise RuntimeError("The registry repeated a publication cursor.")
        seen.add(after)
        filter_by["after"] = after
    if len(matches) != 1:
        raise RuntimeError(f"Publish the pinned {consumer} master artifact before publishing Users evidence.")
    publication, = matches
    if hashlib.sha256(call(f"/api/artifacts/{digest}/content", raw=True)).hexdigest() != digest:
        raise RuntimeError("The registry bytes differ from the verified Pact.")

    configuration = hashlib.sha256()
    for file in (
        ACCEPTANCE / "ECommerceStoreUsers.AcceptanceTests.csproj",
        ACCEPTANCE / "acceptance.runsettings",
        ACCEPTANCE / "ApplicationFactory.cs",
        ACCEPTANCE / "AcceptanceMongoDb.cs",
        ACCEPTANCE / "Configuration/UsersPactProviderTests.cs",
    ):
        configuration.update(hashlib.sha256(file.read_bytes()).digest())

    run = os.environ["GITHUB_RUN_ID"]
    build_url = f"{REPOSITORY}/actions/runs/{run}"
    call("/api/applications/users/versions", {"commitSha": sha, "repositoryUrl": REPOSITORY})
    receipt = call("/api/verification-runs", {
        **report, "consumerPublicationId": publication["id"],
        "consumerApplicationId": consumer, "providerApplicationId": "users",
        "configurationSha256": configuration.hexdigest(), "ciRunId": run,
        "ciAttempt": int(os.environ["GITHUB_RUN_ATTEMPT"]), "ciCheckId": CHECK_IDS[consumer],
        "sourceRef": ref, "buildUrl": build_url, "evidenceUrls": [build_url],
    })
    if (
        receipt["contractSha256"] != digest or receipt["status"] != "Passed" or receipt["publisherId"] != "users-ci"
        or receipt["consumerPublicationId"] != publication["id"] or receipt["consumerCommitSha"] != source["commitSha"]
        or receipt["providerCommitSha"] != sha or receipt["verifiedInteractions"] != source["interactionCount"]
    ):
        raise RuntimeError("The registry returned different Users verification evidence.")
    args.receipt.write_text(json.dumps(receipt, indent=2) + "\n", encoding="utf-8")
    print(f"Published Users verification {receipt['id']} ({receipt['status']}).")


if __name__ == "__main__":
    main()
