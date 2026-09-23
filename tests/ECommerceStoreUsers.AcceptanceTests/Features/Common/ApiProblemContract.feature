Feature: Public API error contract
  Framework failures and unhandled exceptions expose a stable, safe problem shape.

  Scenario Outline: Framework failures have the same JSON problem contract
    When I trigger the Users REF-06 error case "<Case>"
    Then the Users REF-06 response has status <Status> and code "<Code>"

    Examples:
      | Case    | Status | Code                   |
      | route   | 404    | route_not_found        |
      | method  | 405    | method_not_allowed     |
      | media   | 415    | unsupported_media_type |
      | json    | 400    | invalid_json           |
      | type    | 400    | invalid_json           |
      | null    | 400    | invalid_json           |
      | missing | 400    | invalid_json           |
      | body    | 400    | invalid_request        |
