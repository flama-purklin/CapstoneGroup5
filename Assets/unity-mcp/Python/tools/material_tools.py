from mcp.server.fastmcp import FastMCP, Context
from typing import List, Optional

def register_material_tools(mcp: FastMCP):
    """Register stub material-related tools with the MCP server.
    Material functionality has been removed to eliminate URP dependencies.
    """
    
    @mcp.tool()
    def set_material(
        ctx: Context,
        object_name: str,
        material_name: Optional[str] = None,
        color: Optional[List[float]] = None,
        create_if_missing: bool = True
    ) -> str:
        """
        [REMOVED FUNCTIONALITY] Apply or create a material for a game object.
        
        This function is a stub implementation since material functionality has been
        removed to eliminate URP rendering pipeline dependencies.
        
        Args:
            object_name: Target game object.
            material_name: Optional material name.
            color: Optional [R, G, B] or [R, G, B, A] values.
            create_if_missing: Whether to create the material if it doesn't exist.
            
        Returns:
            str: Error message explaining the removal of this functionality.
        """
        return (
            "Material functionality has been removed to eliminate URP dependencies. "
            "Please use the standard Unity editor to manage materials or implement a custom "
            "material handler compatible with your rendering pipeline."
        )