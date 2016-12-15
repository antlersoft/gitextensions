using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GitUI;
using GitUIPluginInterfaces;
using ResourceManager;
using GitUI.RevisionGridClasses;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace FogBugz
{
  public class FogBugzPlugin : GitPluginBase
  {
    private IGitUICommands _gitUiCommands;
    private RevisionGrid _grid;
    private DvcsGraph _graph;
    private ToolStripMenuItem _menuItem;

    private static readonly Regex BugzPattern = new Regex("BugzI[dD]: *([0-9]+)");

    public override string Description
    {
      get
      {
        return "FogBugz - link to case id";
      }
    }

    public FogBugzPlugin()
    : base()
    {
    }

    public override void Register(IGitUICommands gitUiCommands)
    {
      base.Register(gitUiCommands);
      _gitUiCommands = gitUiCommands;
      //gitUiCommands.PostBrowseInitialize += gitUiCommands_PostBrowseInitialize;
      gitUiCommands.PostRegisterPlugin += gitUiCommands_PostRegisterPlugin;
      //gitUiCommands.PostBrowse += gitUiCommands_PostBrowse;
    }

    public override void Unregister(IGitUICommands gitUiCommands)
    {
      //gitUiCommands.PostBrowseInitialize -= gitUiCommands_PostBrowseInitialize;
      gitUiCommands.PostRegisterPlugin -= gitUiCommands_PostRegisterPlugin;
      //gitUiCommands.PostBrowse -= gitUiCommands_PostBrowse;
      _gitUiCommands = null;
      base.Unregister(gitUiCommands);
    }

    void gitUiCommands_PostRegisterPlugin(object sender, GitUIBaseEventArgs e)
    {
      UpdateMenuItems(e);
    }

    void gitUiCommands_PostBrowseInitialize(object sender, GitUIBaseEventArgs e)
    {
      UpdateMenuItems(e);
    }

    void gitUiCommands_PostBrowse(object sender, GitUIBaseEventArgs e)
    {
      UpdateMenuItems(e);
    }

    void UpdateMenuItems(GitUIBaseEventArgs e)
    {
      Form form = (Form)e.OwnerForm;
      RevisionGrid grid = FindControl<RevisionGrid>(form, p => true);
      if (grid == _grid) return;
      _grid = grid;
      _graph = FindControl<DvcsGraph>(form, p => true);
      _menuItem = new ToolStripMenuItem("FogBugz Case");
      _menuItem.Click += _menuItem_Click;
      _graph.ContextMenuStrip.Items.AddRange(new ToolStripItem[] { _menuItem });
      _grid.SelectionChanged += _grid_SelectionChanged;
      //_grid.SelectionChanged += _grid_SelectionChanged;
    }

    private void _grid_SelectionChanged(object sender, EventArgs e)
    {
      _menuItem.Enabled = _grid.GetSelectedRevisions().Any(gr => BugzPattern.IsMatch(gr.Body+gr.Message));
    }

    private void _menuItem_Click(object sender, EventArgs e)
    {
      foreach (var gr in _grid.GetSelectedRevisions())
      {
        var match = BugzPattern.Match(gr.Message + gr.Body);
        if (match.Success)
        {
          ProcessStartInfo psi = new ProcessStartInfo($"http://olfogbugz/default.asp?{match.Groups[1]}");
          Process.Start(psi);
        }
      }
    }

    public override bool Execute(GitUIBaseEventArgs gitUiCommands)
    {
      return true;
    }
    private T FindControl<T>(Control form, Func<T, bool> predicate)
      where T : Control
    {
      return FindControl(form.Controls, predicate);
    }

    private T FindControl<T>(IEnumerable controls, Func<T, bool> predicate)
        where T : Control
    {
      foreach (Control control in controls)
      {
        var result = control as T;

        if (result != null && predicate(result))
          return result;

        result = FindControl(control.Controls, predicate);

        if (result != null)
          return result;
      }

      return null;
    }
  }

}
