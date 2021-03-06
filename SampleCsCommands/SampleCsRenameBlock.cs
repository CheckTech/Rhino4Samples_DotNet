using RMA.Rhino;
using RMA.OpenNURBS;

namespace SampleCsCommands
{
  ///<summary>
  /// A Rhino.NET plug-in can contain as many MRhinoCommand derived classes as it wants.
  /// DO NOT create an instance of this class (this is the responsibility of Rhino.NET.)
  /// </summary>
  public class SampleCsRenameBlock : RMA.Rhino.MRhinoCommand
  {
    ///<summary>
    /// Rhino tracks commands by their unique ID. Every command must have a unique id.
    /// The Guid created by the project wizard is unique. You can create more Guids using
    /// the "Create Guid" tool in the Tools menu.
    ///</summary>
    ///<returns>The id for this command</returns>
    public override System.Guid CommandUUID()
    {
      return new System.Guid("{dbf87b15-8657-4164-a26a-4e76fdddf50e}");
    }

    ///<returns>The command name as it appears on the Rhino command line</returns>
    public override string EnglishCommandName()
    {
      return "SampleCsRenameBlock";
    }

    public enum idef_settings : uint
    {
      no_idef_settings = 0x0,
      idef_name_setting = 0x1,
      idef_description_setting = 0x2,
      idef_url_setting = 0x4,
      idef_units_setting = 0x8,
      idef_source_archive_setting = 0x10,
      idef_userdata_setting = 0x20,
      all_idef_settings = 0xFFFFFFFF
    }

    ///<summary> This gets called when when the user runs this command.</summary>
    public override IRhinoCommand.result RunCommand(IRhinoCommandContext context)
    {
      // Get the name of the instance definition to rename
      MRhinoGetString gs = new MRhinoGetString();
      gs.SetCommandPrompt("Name of block to rename");
      gs.GetString();
      if (gs.CommandResult() != IRhinoCommand.result.success)
        return gs.CommandResult();

      // Validate string
      string idef_name = gs.String().Trim();
      if (string.IsNullOrEmpty(idef_name))
        return IRhinoCommand.result.nothing;

      // Verify instance definition exists
      MRhinoInstanceDefinitionTable idef_table = context.m_doc.m_instance_definition_table;
      int idef_index = idef_table.FindInstanceDefinition(idef_name);
      if (idef_index < 0)
      {
        RhUtil.RhinoApp().Print(string.Format("Block \"{0}\" not found.\n", idef_name));
        return IRhinoCommand.result.nothing;
      }

      // Verify instance definition is rename-able
      IRhinoInstanceDefinition idef = idef_table[idef_index];
      if (idef.IsDeleted() || idef.IsReference())
      {
        RhUtil.RhinoApp().Print(string.Format("Unable to rename block \"{0}\".\n", idef_name));
        return IRhinoCommand.result.nothing;
      }

      // Get the new instance definition name
      gs.SetCommandPrompt("New block name");
      gs.GetString();
      if (gs.CommandResult() != IRhinoCommand.result.success)
        return gs.CommandResult();

      // Validate string
      string new_idef_name = gs.String().Trim();
      if (string.IsNullOrEmpty(new_idef_name))
        return IRhinoCommand.result.nothing;

      // Verify the new instance definition name is not already in use
      int new_idef_index = idef_table.FindInstanceDefinition(new_idef_name);
      if (new_idef_index >= 0 && new_idef_index < idef_table.InstanceDefinitionCount())
      {
        if (!idef_table[new_idef_index].IsDeleted())
        {
          RhUtil.RhinoApp().Print(string.Format("Block \"{0}\" already exists.\n", new_idef_name));
          return IRhinoCommand.result.nothing;
        }
      }

      // Make a copy of the instance definition object
      OnInstanceDefinition new_idef = new OnInstanceDefinition(idef);

      // Rename the copy
      new_idef.SetName(new_idef_name);

      // Modify the existing instance definition with our copy
      if (idef_table.ModifyInstanceDefinition(new_idef, idef_index, (uint)idef_settings.idef_name_setting, false))
        return IRhinoCommand.result.success;

      return IRhinoCommand.result.failure;
    }
  }
}

