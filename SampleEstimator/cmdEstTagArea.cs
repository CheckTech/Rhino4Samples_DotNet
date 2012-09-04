/////////////////////////////////////////////////////////////////////////////
// cmdEstTagArea.cs

using RMA.Rhino;
using RMA.OpenNURBS;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Estimator
{
  class RhinoGetAreaObjects : RMA.Rhino.MRhinoGetObject
  {
    public override bool CustomGeometryFilter(IRhinoObject obj, IOnGeometry geo, OnCOMPONENT_INDEX ci)
    {
      if (geo != null)
      {
        IOnCurve crv = OnCurve.ConstCast(geo);
        if (crv != null)
        {
          if (crv.IsClosed() && crv.IsPlanar())
            return true;
          else
            return false;
        }

        IOnBrep brep = OnBrep.ConstCast(geo);
        if (brep != null)
        {
          if (brep.m_F.Count() == 1)
            return true;
          else
            return false;
        }

        IOnSurface srf = OnSurface.ConstCast(geo);
        if (srf != null)
          return true;

        IOnMesh mesh = OnMesh.ConstCast(geo);
        if (mesh != null)
          return true;
      }

      return false;
    }
  }

  ///<summary>
  /// A Rhino.NET plug-in can contain as many MRhinoCommand derived classes as it wants.
  /// DO NOT create an instance of this class (this is the responsibility of Rhino.NET.)
  /// A command wizard can be found in visual studio when adding a new item to the project.
  /// </summary>
  public class EstTagAreaCommand : RMA.Rhino.MRhinoCommand
  {
    ///<summary>
    /// Rhino tracks commands by their unique ID. Every command must have a unique id.
    /// The Guid created by the project wizard is unique. You can create more Guids using
    /// the "Create Guid" tool in the Tools menu.
    ///</summary>
    ///<returns>The id for this command</returns>
    public override System.Guid CommandUUID()
    {
      return new System.Guid("{B705B75C-10E7-4098-9950-6F434E88862A}");
    }

    ///<returns>The command name as it appears on the Rhino command line</returns>
    public override string EnglishCommandName()
    {
      return "EstTagArea";
    }

    ///<summary> This gets called when when the user runs this command.</summary>
    public override IRhinoCommand.result RunCommand(IRhinoCommandContext context)
    {
      RhinoGetAreaObjects go = new RhinoGetAreaObjects();
      go.SetCommandPrompt("Select objects to apply area tags");
      go.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object | IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object | IRhinoGetObject.GEOMETRY_TYPE_FILTER.mesh_object);
      go.EnableSubObjectSelect(false);
      go.GetObjects(1, 0);
      if (go.CommandResult() != IRhinoCommand.result.success)
        return go.CommandResult();

      EstimatorTagForm form = new EstimatorTagForm();
      form.m_str_title = "Add Area Tags";
      form.m_str_prompt = "Select one or more area tags.";
      form.m_type = EstimatorTag.tag_type.area_tag;

      DialogResult rc = form.ShowDialog();
      if (rc == DialogResult.Cancel)
        return IRhinoCommand.result.cancel;

      List<string> tag_strings = new List<string>(form.m_selected_tags.Count);

      EstimatorPlugIn plugin = RMA.Rhino.RhUtil.GetPlugInInstance() as EstimatorPlugIn;
      int i = 0;

      for (i = 0; i < form.m_selected_tags.Count; i++)
      {
        int index = form.m_selected_tags[i];
        tag_strings.Add(plugin.m_tag_table[index].Id());
      }

      for (i = 0; i < go.ObjectCount(); i++)
        EstimatorHelpers.AddData(go.Object(i).Object(), tag_strings.ToArray());

      return IRhinoCommand.result.success;
    }
  }
}

