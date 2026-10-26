Feature: Public API error contract
  Framework failures and unhandled exceptions expose a stable, safe problem shape.

  Scenario Outline: Framework failures have the same JSON problem contract
    A request fails during routing, method selection, body binding or media-type handling.
    Each example checks its status and safe problem code; malformed or incomplete bodies do not create business data.
    When I trigger the Users REF-06 error case "<Case>"
    Then the Users REF-06 response has status <Status> and code "<Code>"

    Examples:
      | Case    | Status | Code                   | scenarioId                          |
      | route   | 404    | route_not_found        | REF06_Framework_Route               |
      | method  | 405    | method_not_allowed     | REF06_Framework_Method              |
      | media   | 415    | unsupported_media_type | REF06_Framework_Media               |
      | json    | 400    | invalid_json           | REF06_Framework_Json                |
      | type    | 400    | invalid_json           | REF07_AddProductToFavorites_type    |
      | null    | 400    | invalid_json           | REF07_AddProductToFavorites_null    |
      | missing | 400    | invalid_json           | REF07_AddProductToFavorites_missing |
      | body    | 400    | invalid_request        | REF06_Framework_Body                |
