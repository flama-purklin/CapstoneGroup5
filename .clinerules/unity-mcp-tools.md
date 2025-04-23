# Unity MCP Tools Reference

### execute_menu_item
```json
{"menu_path":"GameObject/Create Empty","action":"execute"}
```
- `menu_path`: [REQ] String - Menu path
- `action`: [OPT] String - "execute" (default) or "get_available_menus"
- `parameters`: [OPT] Object - Additional params

### manage_asset
```json
{"action":"create","path":"Assets/Materials/New.mat","asset_type":"Material","properties":{"color":[1,0,0,1]}}
```
- `action`: [REQ] String - "import", "create", "modify", "delete", "duplicate", "move", "rename", "search", "get_info", "create_folder", "get_components"
- `path`: [REQ] String - Asset path
- `asset_type`: [REQ-create] String - Asset type for creation
- `properties`: [OPT] Object - Properties to set
- `destination`: [OPT] String - Target for move/duplicate
- Search options: `search_pattern`, `filter_type`, `filter_date_after`, `page_size`, `page_number`

### manage_editor
```json
{"action":"play","wait_for_completion":true}
```
- `action`: [REQ] String - "play", "pause", "stop", "get_state", "set_active_tool", "add_tag"
- `wait_for_completion`: [OPT] Boolean - Wait for action
- Action-specific: `tool_name`, `tag_name`, `layer_name`

### manage_gameobject
```json
{"action":"create","name":"Player","position":[0,1,0],"components_to_add":["Rigidbody"]}
```
- `action`: [REQ] String - "create", "modify", "delete", "find", "get_components", "add_component", "remove_component", "set_component_property"
- `target`: [REQ-most actions] String/Int - GameObject identifier
- `search_method`: [OPT] String - "by_name", "by_id", "by_path"
- Core props: `name`, `tag`, `parent`, `position`:[x,y,z], `rotation`, `scale`, `set_active`, `layer`
- Component props: `components_to_add`:[strings], `components_to_remove`, `component_name`, `component_properties`:{obj}
- Creation: `primitive_type`, `save_as_prefab`, `prefab_path`
- Search: `search_term`, `find_all`, `search_in_children`, `search_inactive`

### manage_scene
```json
{"action":"load","name":"MainScene","path":"Assets/Scenes","build_index":0}
```
- `action`: [REQ] String - "load", "save", "create", "get_hierarchy"
- `name`: [REQ] String - Scene name (no extension)
- `path`: [REQ] String - Asset path
- `build_index`: [REQ] Int - Build index for actions

### manage_script
```json
{"action":"create","name":"PlayerController","path":"Assets/Scripts","contents":"using UnityEngine;\npublic class PlayerController : MonoBehaviour {}","script_type":"MonoBehaviour","namespace":"MyGame"}
```
- `action`: [REQ] String - "create", "read", "update", "delete"
- `name`: [REQ] String - Script name (no extension)
- `path`: [REQ] String - Asset path
- `contents`: [REQ-create/update] String - C# code
- `script_type`: [REQ] String - Type hint ("MonoBehaviour")
- `namespace`: [REQ] String - Script namespace

### read_console
```json
{"action":"get","types":["error","warning"],"count":10}
```
- `action`: [OPT] String - "get" (default), "clear"
- `types`: [OPT] String[] - Default ["error","warning","log"]
- `count`: [OPT] Int - Max messages
- `filter_text`: [OPT] String - Filter by text
- `since_timestamp`: [OPT] String - ISO 8601 datetime
- `format`: [OPT] String - "plain", "detailed" (default), "json"
- `include_stacktrace`: [OPT] Boolean - Default true

## Notes
- Snake_case Python side parameters, camelCase Unity side
- Null parameters omitted from requests
- String values normalized to lowercase
- Check response "success" key for operation status