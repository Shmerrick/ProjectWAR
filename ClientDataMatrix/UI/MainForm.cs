using ClientDataMatrix.Model;
using ClientDataMatrix.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientDataMatrix.UI
{
    public sealed class MainForm : Form
    {
        private TextBox _rootPathTextBox;
        private TextBox _outputPathTextBox;
        private Button _reloadButton;
        private Button _generateAllButton;
        private Button _cleanWorkspaceButton;
        private TextBox _abilitySearchTextBox;
        private CheckBox _hidePartialAbilitiesCheckBox;
        private Label _abilityCatalogStatusLabel;
        private DataGridView _abilityGrid;
        private TextBox _abilityIdTextBox;
        private Button _generateAbilityButton;
        private Button _openAbilityMarkdownButton;
        private Button _openAbilityFolderButton;
        private TextBox _abilitySummaryTextBox;
        private TreeView _abilityTreeView;
        private TextBox _abilityNarrativeTextBox;
        private DataGridView _definitionGrid;
        private Label _definitionHintLabel;
        private Button _generateTokenDictionaryButton;
        private Button _openTokenDictionaryMarkdownButton;
        private Button _openTokenDictionaryFolderButton;
        private TextBox _tokenSummaryTextBox;
        private DataGridView _tokenGrid;
        private Button _generateCoverageButton;
        private Button _openCoverageMarkdownButton;
        private Button _openCoverageFolderButton;
        private TextBox _coverageSummaryTextBox;
        private DataGridView _coverageGrid;
        private Button _generateRemainingWorkButton;
        private Button _openRemainingWorkMarkdownButton;
        private Button _openRemainingWorkNextMarkdownButton;
        private Button _openRemainingWorkOperationFieldsMarkdownButton;
        private Button _openRemainingWorkLiteralCrosswalkMarkdownButton;
        private Button _openRemainingWorkFolderButton;
        private CheckBox _remainingWorkAllAreasCheckBox;
        private ComboBox _remainingWorkPriorityFilterComboBox;
        private NumericUpDown _remainingWorkTopCountUpDown;
        private TextBox _remainingWorkSearchTextBox;
        private TextBox _remainingWorkSummaryTextBox;
        private DataGridView _remainingWorkAreaGrid;
        private DataGridView _remainingWorkItemGrid;
        private TextBox _remainingWorkDetailTextBox;
        private Button _generateDomainsButton;
        private Button _openDomainsMarkdownButton;
        private Button _openDomainsFolderButton;
        private TextBox _domainsSummaryTextBox;
        private DataGridView _domainsGrid;
        private DataGridView _domainValuesGrid;
        private Button _generateRequirementsButton;
        private Button _openRequirementsMarkdownButton;
        private Button _openRequirementsFolderButton;
        private TextBox _requirementsSummaryTextBox;
        private DataGridView _requirementsGrid;
        private DataGridView _requirementRowGrid;
        private DataGridView _requirementFieldGrid;
        private DataGridView _requirementReferenceGrid;
        private Button _generateOperationSchemasButton;
        private Button _openOperationSchemasMarkdownButton;
        private Button _openOperationSchemasFolderButton;
        private TextBox _operationSummaryTextBox;
        private DataGridView _operationGrid;
        private DataGridView _operationFieldGrid;
        private DataGridView _operationAbilityGrid;
        private Button _generateUnknownsButton;
        private Button _openUnknownMarkdownButton;
        private Button _openUnknownFolderButton;
        private CheckBox _hideMultiplierUnknownsCheckBox;
        private TextBox _unknownSummaryTextBox;
        private DataGridView _unknownFieldGrid;
        private DataGridView _unknownValueGrid;
        private TextBox _unknownInsightTextBox;
        private DataGridView _unknownCompanionGrid;
        private DataGridView _unknownAbilityGrid;
        private Button _generateConflictsButton;
        private Button _openConflictMarkdownButton;
        private Button _openConflictFolderButton;
        private CheckBox _hideBlankStringConflictCheckBox;
        private CheckBox _hideMirrorEffectIdConflictCheckBox;
        private CheckBox _highSignalConflictsOnlyCheckBox;
        private TextBox _conflictSummaryTextBox;
        private DataGridView _conflictDomainGrid;
        private DataGridView _conflictGrid;
        private TextBox _conflictInsightTextBox;
        private DataGridView _conflictValueGrid;
        private DataGridView _conflictClaimGrid;
        private DataGridView _statusGrid;
        private TextBox _logTextBox;
        private ToolStripStatusLabel _statusLabel;
        private TabControl _mainTabs;
        private ItemArtTab _itemArtTab;
        private CrosswalkTab _crosswalkTab;
        private ClientSearchTab _clientSearchTab;

        private MatrixAnalysisSession _session;
        private DefinitionCatalog _definitions;
        private AbilityNarrativeBuilder _narrativeBuilder;
        private List<AbilityCatalogEntry> _abilityCatalog = new List<AbilityCatalogEntry>();
        private AbilityAnalysisResult _lastAbilityReport;
        private TokenDictionaryDocument _lastTokenDictionary;
        private CoverageReportDocument _lastCoverageReport;
        private RemainingWorkDocument _lastRemainingWorkReport;
        private DomainLedgerDocument _lastDomainLedger;
        private RequirementLedgerDocument _lastRequirementLedger;
        private OperationSchemaDocument _lastOperationSchema;
        private ConflictReportDocument _lastConflictReport;
        private string _lastAbilityMarkdownPath;
        private string _lastTokenDictionaryMarkdownPath;
        private string _lastCoverageMarkdownPath;
        private string _lastRemainingWorkMarkdownPath;
        private string _lastRemainingWorkNextMarkdownPath;
        private string _lastRemainingWorkOperationFieldsMarkdownPath;
        private string _lastRemainingWorkLiteralCrosswalkMarkdownPath;
        private string _lastDomainLedgerMarkdownPath;
        private string _lastRequirementMarkdownPath;
        private string _lastOperationSchemaMarkdownPath;
        private string _lastConflictMarkdownPath;
        private bool _isBusy;
        private readonly Dictionary<string, List<ComponentOperationFieldValueRecord>> _unknownValueCache = new Dictionary<string, List<ComponentOperationFieldValueRecord>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ComponentOperationFieldValueInsightRecord> _unknownInsightCache = new Dictionary<string, ComponentOperationFieldValueInsightRecord>(StringComparer.OrdinalIgnoreCase);

        public MainForm(string extractedRootPath, string outputRoot)
        {
            Text = "ClientDataMatrix";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1360, 860);
            Size = new Size(1500, 940);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            TableLayoutPanel shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shell.Controls.Add(CreatePathPanel(extractedRootPath, outputRoot), 0, 0);
            shell.Controls.Add(CreateMainTabs(), 0, 1);

            StatusStrip strip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel { Text = "Waiting to load extracted dataset..." };
            strip.Items.Add(_statusLabel);
            shell.Controls.Add(strip, 0, 2);

            Controls.Add(shell);
            Shown += async (sender, args) => await LoadSessionAsync();
        }

        private Control CreatePathPanel(string extractedRootPath, string outputRoot)
        {
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 6, RowCount = 2, Padding = new Padding(10, 10, 10, 4) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            panel.Controls.Add(new Label { Text = "Extracted root", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 8, 0) }, 0, 0);
            _rootPathTextBox = new TextBox { Dock = DockStyle.Fill, Text = extractedRootPath ?? string.Empty, Margin = new Padding(0, 4, 8, 4) };
            panel.Controls.Add(_rootPathTextBox, 1, 0);
            Button browseRoot = new Button { Text = "Browse...", AutoSize = true, Margin = new Padding(0, 3, 8, 3) };
            browseRoot.Click += (sender, args) => ChooseFolder(_rootPathTextBox, "Select the extracted client root folder.");
            panel.Controls.Add(browseRoot, 2, 0);
            _reloadButton = new Button { Text = "Reload Data", AutoSize = true };
            _reloadButton.Click += async (sender, args) => await LoadSessionAsync();
            panel.Controls.Add(_reloadButton, 3, 0);
            _generateAllButton = new Button { Text = "Generate All", AutoSize = true, Enabled = false };
            _generateAllButton.Click += async (sender, args) => await GenerateAllReportsAsync();
            panel.Controls.Add(_generateAllButton, 4, 0);
            _cleanWorkspaceButton = new Button { Text = "Clean Temp", AutoSize = true };
            _cleanWorkspaceButton.Click += (sender, args) => CleanWorkspace();
            panel.Controls.Add(_cleanWorkspaceButton, 5, 0);

            panel.Controls.Add(new Label { Text = "Output root", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 8, 0) }, 0, 1);
            _outputPathTextBox = new TextBox { Dock = DockStyle.Fill, Text = outputRoot ?? string.Empty, Margin = new Padding(0, 4, 8, 4) };
            panel.Controls.Add(_outputPathTextBox, 1, 1);
            Button browseOutput = new Button { Text = "Browse...", AutoSize = true, Margin = new Padding(0, 3, 8, 3) };
            browseOutput.Click += (sender, args) => ChooseFolder(_outputPathTextBox, "Select the report output folder.");
            panel.Controls.Add(browseOutput, 2, 1);
            Label hint = new Label { Text = "GUI analysis stays read-only against extracted client files.", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0) };
            panel.Controls.Add(hint, 3, 1);
            panel.SetColumnSpan(hint, 3);

            return panel;
        }

        private Control CreateMainTabs()
        {
            _mainTabs = new TabControl { Dock = DockStyle.Fill };
            _mainTabs.TabPages.Add(CreateAbilityTab());
            _mainTabs.TabPages.Add(CreateTokenTab());
            _mainTabs.TabPages.Add(CreateDomainTab());
            _mainTabs.TabPages.Add(CreateRequirementTab());
            _mainTabs.TabPages.Add(CreateCoverageTab());
            _mainTabs.TabPages.Add(CreateRemainingWorkTab());
            _mainTabs.TabPages.Add(CreateOperationTab());
            _mainTabs.TabPages.Add(CreateUnknownTab());
            _mainTabs.TabPages.Add(CreateConflictTab());

            // These three read the client (and, for the crosswalk, the world database) directly and
            // need none of the ability dataset, so they work before Reload Data has finished.
            string root = _rootPathTextBox == null ? null : _rootPathTextBox.Text;
            string output = _outputPathTextBox == null ? null : _outputPathTextBox.Text;

            _itemArtTab = new ItemArtTab();
            _itemArtTab.Bind(root);
            _mainTabs.TabPages.Add(_itemArtTab.Page);

            _crosswalkTab = new CrosswalkTab();
            _crosswalkTab.Bind(root, output);
            _mainTabs.TabPages.Add(_crosswalkTab.Page);

            _clientSearchTab = new ClientSearchTab();
            _clientSearchTab.Bind(root, output);
            _mainTabs.TabPages.Add(_clientSearchTab.Page);

            _mainTabs.TabPages.Add(CreateStatusTab());
            _mainTabs.TabPages.Add(CreateLogTab());
            return _mainTabs;
        }

        private TabPage CreateAbilityTab()
        {
            TabPage tab = new TabPage("Ability Doctor");
            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 430 };
            split.Panel1.Controls.Add(CreateAbilityCatalogGroup());
            split.Panel2.Controls.Add(CreateAbilityReportGroup());
            tab.Controls.Add(split);
            return tab;
        }

        private Control CreateAbilityCatalogGroup()
        {
            GroupBox left = new GroupBox { Text = "Ability Catalog", Dock = DockStyle.Fill, Padding = new Padding(10) };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel search = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            search.Controls.Add(new Label { Text = "Filter", AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            _abilitySearchTextBox = new TextBox { Width = 320 };
            _abilitySearchTextBox.TextChanged += (sender, args) => ApplyAbilityFilter();
            search.Controls.Add(_abilitySearchTextBox);

            _hidePartialAbilitiesCheckBox = new CheckBox { Text = "Hide Partial/StringsOnly", AutoSize = true, Margin = new Padding(8, 6, 0, 0), Checked = true };
            _hidePartialAbilitiesCheckBox.CheckedChanged += (sender, args) => ApplyAbilityFilter();
            search.Controls.Add(_hidePartialAbilitiesCheckBox);

            _abilityCatalogStatusLabel = new Label { AutoSize = true, Margin = new Padding(0, 0, 0, 8), Text = "Load the extracted dataset to populate the catalog." };
            _abilityGrid = CreateReadOnlyGrid();
            _abilityGrid.AutoGenerateColumns = false;
            _abilityGrid.Columns.Add(CreateTextColumn("AbilityId", "AbilityId", 90));
            _abilityGrid.Columns.Add(CreateTextColumn("Name", "Name", 230));
            _abilityGrid.Columns.Add(CreateTextColumn("Effect", "EffectIdText", 80));
            _abilityGrid.Columns.Add(CreateTextColumn("Sources", "Sources", 100));
            _abilityGrid.SelectionChanged += (sender, args) => { AbilityCatalogEntry selected = SelectedAbility(); if (selected != null) _abilityIdTextBox.Text = selected.AbilityId.ToString(CultureInfo.InvariantCulture); };
            _abilityGrid.CellDoubleClick += async (sender, args) => { if (args.RowIndex >= 0) await GenerateAbilityReportAsync(); };

            layout.Controls.Add(search, 0, 0);
            layout.Controls.Add(_abilityCatalogStatusLabel, 0, 1);
            layout.Controls.Add(_abilityGrid, 0, 2);
            left.Controls.Add(layout);
            return left;
        }

        private Control CreateAbilityReportGroup()
        {
            GroupBox right = new GroupBox { Text = "Ability Report", Dock = DockStyle.Fill, Padding = new Padding(10) };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            actions.Controls.Add(new Label { Text = "Ability ID", AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            _abilityIdTextBox = new TextBox { Width = 110 };
            actions.Controls.Add(_abilityIdTextBox);
            _generateAbilityButton = new Button { Text = "Generate Ability Report", AutoSize = true, Enabled = false };
            _generateAbilityButton.Click += async (sender, args) => await GenerateAbilityReportAsync();
            actions.Controls.Add(_generateAbilityButton);
            _openAbilityMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openAbilityMarkdownButton.Click += (sender, args) => OpenPath(_lastAbilityMarkdownPath);
            actions.Controls.Add(_openAbilityMarkdownButton);
            _openAbilityFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openAbilityFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastAbilityMarkdownPath) ? null : Path.GetDirectoryName(_lastAbilityMarkdownPath));
            actions.Controls.Add(_openAbilityFolderButton);

            _abilitySummaryTextBox = CreateReadOnlyTextBox();
            _abilitySummaryTextBox.Text = "Generate an ability report to populate the tree, the narrative, and decoded field definitions.";

            TabControl reportTabs = new TabControl { Dock = DockStyle.Fill };
            _abilityTreeView = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
            TabPage treeTab = new TabPage("Tree");
            treeTab.Controls.Add(_abilityTreeView);
            reportTabs.TabPages.Add(treeTab);

            _abilityNarrativeTextBox = CreateReadOnlyTextBox();
            _abilityNarrativeTextBox.Text = "Generate an ability report to see what we think happens when the ability is used.";
            TabPage narrativeTab = new TabPage("What Happens");
            narrativeTab.Controls.Add(_abilityNarrativeTextBox);
            reportTabs.TabPages.Add(narrativeTab);

            TabPage defTab = new TabPage("Definitions");
            defTab.Controls.Add(CreateDefinitionPanel());
            reportTabs.TabPages.Add(defTab);

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_abilitySummaryTextBox, 0, 1);
            layout.Controls.Add(reportTabs, 0, 2);
            right.Controls.Add(layout);
            return right;
        }

        private TabPage CreateConflictTab()
        {
            TabPage tab = new TabPage("Conflict Ledger");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateConflictsButton = new Button { Text = "Generate Conflict Report", AutoSize = true, Enabled = false };
            _generateConflictsButton.Click += async (sender, args) => await GenerateConflictReportAsync();
            actions.Controls.Add(_generateConflictsButton);
            _openConflictMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openConflictMarkdownButton.Click += (sender, args) => OpenPath(_lastConflictMarkdownPath);
            actions.Controls.Add(_openConflictMarkdownButton);
            _openConflictFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openConflictFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastConflictMarkdownPath) ? null : Path.GetDirectoryName(_lastConflictMarkdownPath));
            actions.Controls.Add(_openConflictFolderButton);
            _hideBlankStringConflictCheckBox = new CheckBox { Text = "Hide Blank String Noise", AutoSize = true };
            _hideBlankStringConflictCheckBox.CheckedChanged += (sender, args) => RefreshConflictView();
            actions.Controls.Add(_hideBlankStringConflictCheckBox);
            _hideMirrorEffectIdConflictCheckBox = new CheckBox { Text = "Hide AbilityId-Mirror EffectId Pattern", AutoSize = true, Checked = true };
            _hideMirrorEffectIdConflictCheckBox.CheckedChanged += (sender, args) => RefreshConflictView();
            actions.Controls.Add(_hideMirrorEffectIdConflictCheckBox);
            _highSignalConflictsOnlyCheckBox = new CheckBox { Text = "High-Signal Only", AutoSize = true };
            _highSignalConflictsOnlyCheckBox.CheckedChanged += (sender, args) => RefreshConflictView();
            actions.Controls.Add(_highSignalConflictsOnlyCheckBox);

            _conflictSummaryTextBox = CreateReadOnlyTextBox();
            _conflictSummaryTextBox.Text = "Generate the conflict ledger to browse contradiction domains.";

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 330 };
            _conflictDomainGrid = CreateReadOnlyGrid();
            _conflictDomainGrid.AutoGenerateColumns = false;
            _conflictDomainGrid.Columns.Add(CreateTextColumn("Domain", "Domain", 180));
            _conflictDomainGrid.Columns.Add(CreateTextColumn("Peak", "PeakTriageScore", 70));
            _conflictDomainGrid.Columns.Add(CreateTextColumn("High", "HighSignalCount", 70));
            _conflictDomainGrid.Columns.Add(CreateTextColumn("Conflicts", "ConflictCount", 90));
            _conflictDomainGrid.Columns.Add(CreateTextColumn("Subjects", "SubjectCount", 90));
            _conflictDomainGrid.Columns.Add(CreateTextColumn("Facts", "FactCount", 90));
            _conflictDomainGrid.SelectionChanged += (sender, args) => RefreshConflictRows();

            _conflictGrid = CreateReadOnlyGrid();
            _conflictGrid.AutoGenerateColumns = false;
            _conflictGrid.Columns.Add(CreateTextColumn("Triage", "TriageBucket", 80));
            _conflictGrid.Columns.Add(CreateTextColumn("Score", "TriageScore", 65));
            _conflictGrid.Columns.Add(CreateTextColumn("Category", "TriageCategory", 130));
            _conflictGrid.Columns.Add(CreateTextColumn("Resolve To", "ResolutionHint", 180));
            _conflictGrid.Columns.Add(CreateTextColumn("Kind", "SubjectKind", 95));
            _conflictGrid.Columns.Add(CreateTextColumn("Subject", "SubjectKey", 190));
            _conflictGrid.Columns.Add(CreateTextColumn("Fact", "FactName", 140));
            _conflictGrid.Columns.Add(CreateTextColumn("Domain", "Domain", 110));
            _conflictGrid.Columns.Add(CreateTextColumn("Distinct Values", "DistinctValues", 420));
            _conflictGrid.Columns.Add(CreateTextColumn("Evidence", "ClaimCount", 80));
            _conflictGrid.SelectionChanged += (sender, args) => RefreshConflictClaims();
            _conflictGrid.CellDoubleClick += async (sender, args) => { if (args.RowIndex >= 0) await NavigateSelectedConflictAbilityAsync(); };

            _conflictInsightTextBox = CreateReadOnlyTextBox();
            _conflictInsightTextBox.Text = "Select a conflict to inspect its subject, decoded values, and source pattern.";

            _conflictValueGrid = CreateReadOnlyGrid();
            _conflictValueGrid.AutoGenerateColumns = false;
            _conflictValueGrid.Columns.Add(CreateTextColumn("Value", "ValueText", 150));
            _conflictValueGrid.Columns.Add(CreateTextColumn("Meaning", "Meaning", 220));
            _conflictValueGrid.Columns.Add(CreateTextColumn("Sources", "SourcesText", 120));
            _conflictValueGrid.Columns.Add(CreateTextColumn("Claims", "ClaimCount", 70));
            _conflictValueGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 260));

            _conflictClaimGrid = CreateReadOnlyGrid();
            _conflictClaimGrid.AutoGenerateColumns = false;
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Value", "NormalizedValue", 220));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Raw", "RawValue", 160));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Source", "SourceFamily", 110));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("File", "TableName", 160));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Field", "FieldName", 130));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 120));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Path", "SourcePath", 300));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Location", "SourceLocation", 100));
            _conflictClaimGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 220));

            SplitContainer detailSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 290 };
            detailSplit.Panel1.Controls.Add(WrapInGroup("Conflict Details", _conflictGrid));

            SplitContainer evidenceSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 480 };
            TableLayoutPanel profileLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            profileLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            profileLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            profileLayout.Controls.Add(WrapInGroup("Conflict Profile", _conflictInsightTextBox), 0, 0);
            profileLayout.Controls.Add(WrapInGroup("Value Meanings", _conflictValueGrid), 0, 1);
            evidenceSplit.Panel1.Controls.Add(profileLayout);
            evidenceSplit.Panel2.Controls.Add(WrapInGroup("Claim Evidence", _conflictClaimGrid));
            detailSplit.Panel2.Controls.Add(evidenceSplit);

            split.Panel1.Controls.Add(WrapInGroup("Conflict Domains", _conflictDomainGrid));
            split.Panel2.Controls.Add(detailSplit);

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_conflictSummaryTextBox, 0, 1);
            layout.Controls.Add(split, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateUnknownTab()
        {
            TabPage tab = new TabPage("Unknown Triage");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateUnknownsButton = new Button { Text = "Generate Operation Schemas", AutoSize = true, Enabled = false };
            _generateUnknownsButton.Click += async (sender, args) => await GenerateOperationSchemaReportAsync();
            actions.Controls.Add(_generateUnknownsButton);
            _openUnknownMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openUnknownMarkdownButton.Click += (sender, args) => OpenPath(_lastOperationSchemaMarkdownPath);
            actions.Controls.Add(_openUnknownMarkdownButton);
            _openUnknownFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openUnknownFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastOperationSchemaMarkdownPath) ? null : Path.GetDirectoryName(_lastOperationSchemaMarkdownPath));
            actions.Controls.Add(_openUnknownFolderButton);
            _hideMultiplierUnknownsCheckBox = new CheckBox { Text = "Hide Multiplier Noise", AutoSize = true, Checked = true };
            _hideMultiplierUnknownsCheckBox.CheckedChanged += (sender, args) => RefreshUnknownView();
            actions.Controls.Add(_hideMultiplierUnknownsCheckBox);

            _unknownSummaryTextBox = CreateReadOnlyTextBox();
            _unknownSummaryTextBox.Text = "Generate operation schemas to rank the remaining unknown or structural component fields by impact and priority.";

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 360 };
            _unknownFieldGrid = CreateReadOnlyGrid();
            _unknownFieldGrid.AutoGenerateColumns = false;
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Triage", "TriageBucket", 80));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Score", "TriageScore", 65));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 85));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Priority", "PriorityText", 70));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("OperationId", "OperationId", 90));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("OperationName", "OperationName", 180));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Field", "FieldKey", 180));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("NonZero", "NonZeroCount", 75));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Distinct", "DistinctValueCount", 75));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 190));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Sample Values", "SampleValuesText", 220));
            _unknownFieldGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 320));
            _unknownFieldGrid.SelectionChanged += (sender, args) => RefreshUnknownDetails();
            split.Panel1.Controls.Add(WrapInGroup("Unknown Field Hotspots", _unknownFieldGrid));

            SplitContainer detailSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 430 };
            _unknownValueGrid = CreateReadOnlyGrid();
            _unknownValueGrid.AutoGenerateColumns = false;
            _unknownValueGrid.Columns.Add(CreateTextColumn("Raw Value", "RawValue", 90));
            _unknownValueGrid.Columns.Add(CreateTextColumn("Observations", "ObservationCount", 90));
            _unknownValueGrid.Columns.Add(CreateTextColumn("Components", "DistinctComponentCount", 85));
            _unknownValueGrid.Columns.Add(CreateTextColumn("Abilities", "DistinctAbilityCount", 75));
            _unknownValueGrid.Columns.Add(CreateTextColumn("Sample ComponentIds", "SampleComponentIdsText", 160));
            _unknownValueGrid.Columns.Add(CreateTextColumn("Sample AbilityIds", "SampleAbilityIdsText", 160));
            _unknownValueGrid.SelectionChanged += (sender, args) => RefreshUnknownValueAbilities();
            detailSplit.Panel1.Controls.Add(WrapInGroup("Value Evidence", _unknownValueGrid));

            TableLayoutPanel insightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            insightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            insightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));
            insightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _unknownInsightTextBox = CreateReadOnlyTextBox();
            _unknownInsightTextBox.Text = "Select an unknown or structural field and raw value to inspect its trigger mix, context tags, and dominant companion fields.";
            insightLayout.Controls.Add(WrapInGroup("Value Profile", _unknownInsightTextBox), 0, 0);

            _unknownCompanionGrid = CreateReadOnlyGrid();
            _unknownCompanionGrid.AutoGenerateColumns = false;
            _unknownCompanionGrid.Columns.Add(CreateTextColumn("Field", "FieldKey", 170));
            _unknownCompanionGrid.Columns.Add(CreateTextColumn("Dominant Value", "DominantValue", 110));
            _unknownCompanionGrid.Columns.Add(CreateTextColumn("Coverage %", "CoveragePercent", 80));
            _unknownCompanionGrid.Columns.Add(CreateTextColumn("Matches", "MatchCount", 80));
            _unknownCompanionGrid.Columns.Add(CreateTextColumn("Observed", "ObservationCount", 80));
            _unknownCompanionGrid.Columns.Add(CreateTextColumn("Distinct", "DistinctValueCount", 75));
            _unknownCompanionGrid.Columns.Add(CreateTextColumn("Sample Values", "SampleValuesText", 220));
            insightLayout.Controls.Add(WrapInGroup("Correlated Fields", _unknownCompanionGrid), 0, 1);

            _unknownAbilityGrid = CreateReadOnlyGrid();
            _unknownAbilityGrid.AutoGenerateColumns = false;
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("AbilityId", "AbilityId", 90));
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("AbilityName", "AbilityName", 170));
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("ComponentId", "ComponentId", 95));
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("Slot", "ComponentSlotIndex", 60));
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("Trigger", "TriggerText", 170));
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 150));
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("TextExcerpt", "TextExcerpt", 420));
            _unknownAbilityGrid.Columns.Add(CreateTextColumn("Source", "SourceLocation", 90));
            _unknownAbilityGrid.CellDoubleClick += async (sender, args) => { if (args.RowIndex >= 0) await NavigateSelectedUnknownAbilityAsync(); };
            insightLayout.Controls.Add(WrapInGroup("Sample Abilities", _unknownAbilityGrid), 0, 2);
            detailSplit.Panel2.Controls.Add(insightLayout);
            split.Panel2.Controls.Add(detailSplit);

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_unknownSummaryTextBox, 0, 1);
            layout.Controls.Add(split, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateTokenTab()
        {
            TabPage tab = new TabPage("Token Dictionary");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateTokenDictionaryButton = new Button { Text = "Generate Token Dictionary", AutoSize = true, Enabled = false };
            _generateTokenDictionaryButton.Click += async (sender, args) => await GenerateTokenDictionaryAsync();
            actions.Controls.Add(_generateTokenDictionaryButton);
            _openTokenDictionaryMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openTokenDictionaryMarkdownButton.Click += (sender, args) => OpenPath(_lastTokenDictionaryMarkdownPath);
            actions.Controls.Add(_openTokenDictionaryMarkdownButton);
            _openTokenDictionaryFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openTokenDictionaryFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastTokenDictionaryMarkdownPath) ? null : Path.GetDirectoryName(_lastTokenDictionaryMarkdownPath));
            actions.Controls.Add(_openTokenDictionaryFolderButton);

            _tokenSummaryTextBox = CreateReadOnlyTextBox();
            _tokenSummaryTextBox.Text = "Generate the COM token dictionary to write plain-English token definitions from extracted client evidence.";

            _tokenGrid = CreateReadOnlyGrid();
            _tokenGrid.AutoGenerateColumns = false;
            _tokenGrid.Columns.Add(CreateTextColumn("Token", "ExampleToken", 150));
            _tokenGrid.Columns.Add(CreateTextColumn("Field", "FieldKey", 130));
            _tokenGrid.Columns.Add(CreateTextColumn("Meaning", "PlainEnglishMeaning", 420));
            _tokenGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 90));
            _tokenGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 150));
            _tokenGrid.Columns.Add(CreateTextColumn("Abilities", "ExampleAbilityIdsText", 200));
            _tokenGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 260));

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_tokenSummaryTextBox, 0, 1);
            layout.Controls.Add(_tokenGrid, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateDomainTab()
        {
            TabPage tab = new TabPage("Domains");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateDomainsButton = new Button { Text = "Generate Domain Ledger", AutoSize = true, Enabled = false };
            _generateDomainsButton.Click += async (sender, args) => await GenerateDomainLedgerAsync();
            actions.Controls.Add(_generateDomainsButton);
            _openDomainsMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openDomainsMarkdownButton.Click += (sender, args) => OpenPath(_lastDomainLedgerMarkdownPath);
            actions.Controls.Add(_openDomainsMarkdownButton);
            _openDomainsFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openDomainsFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastDomainLedgerMarkdownPath) ? null : Path.GetDirectoryName(_lastDomainLedgerMarkdownPath));
            actions.Controls.Add(_openDomainsFolderButton);

            _domainsSummaryTextBox = CreateReadOnlyTextBox();
            _domainsSummaryTextBox.Text = "Generate the core identity domain ledger to separate proven extracted-client domains such as Race.EntryId, CareerLine.EntryId, and repeated CareerName entry ids.";

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 250 };
            _domainsGrid = CreateReadOnlyGrid();
            _domainsGrid.AutoGenerateColumns = false;
            _domainsGrid.Columns.Add(CreateTextColumn("Domain", "DisplayName", 210));
            _domainsGrid.Columns.Add(CreateTextColumn("Key", "DomainKey", 190));
            _domainsGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 90));
            _domainsGrid.Columns.Add(CreateTextColumn("Canonicality", "Canonicality", 260));
            _domainsGrid.Columns.Add(CreateTextColumn("Values", "ValueCount", 70));
            _domainsGrid.Columns.Add(CreateTextColumn("Distinct Meanings", "DistinctMeaningCount", 95));
            _domainsGrid.Columns.Add(CreateTextColumn("Duplicate Groups", "DuplicateMeaningCount", 95));
            _domainsGrid.Columns.Add(CreateTextColumn("Sources", "SourceFilesText", 160));
            _domainsGrid.SelectionChanged += (sender, args) => RefreshDomainValues();

            _domainValuesGrid = CreateReadOnlyGrid();
            _domainValuesGrid.AutoGenerateColumns = false;
            _domainValuesGrid.Columns.Add(CreateTextColumn("Raw", "RawValue", 80));
            _domainValuesGrid.Columns.Add(CreateTextColumn("Meaning", "Meaning", 220));
            _domainValuesGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 90));
            _domainValuesGrid.Columns.Add(CreateTextColumn("Source", "Source", 150));
            _domainValuesGrid.Columns.Add(CreateTextColumn("Path", "SourcePath", 360));
            _domainValuesGrid.Columns.Add(CreateTextColumn("Location", "SourceLocation", 90));
            _domainValuesGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 320));

            split.Panel1.Controls.Add(WrapInGroup("Domain Summary", _domainsGrid));
            split.Panel2.Controls.Add(WrapInGroup("Domain Values", _domainValuesGrid));

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_domainsSummaryTextBox, 0, 1);
            layout.Controls.Add(split, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateRequirementTab()
        {
            TabPage tab = new TabPage("Requirements");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateRequirementsButton = new Button { Text = "Generate Requirement Ledger", AutoSize = true, Enabled = false };
            _generateRequirementsButton.Click += async (sender, args) => await GenerateRequirementLedgerAsync();
            actions.Controls.Add(_generateRequirementsButton);
            _openRequirementsMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openRequirementsMarkdownButton.Click += (sender, args) => OpenPath(_lastRequirementMarkdownPath);
            actions.Controls.Add(_openRequirementsMarkdownButton);
            _openRequirementsFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openRequirementsFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastRequirementMarkdownPath) ? null : Path.GetDirectoryName(_lastRequirementMarkdownPath));
            actions.Controls.Add(_openRequirementsFolderButton);

            _requirementsSummaryTextBox = CreateReadOnlyTextBox();
            _requirementsSummaryTextBox.Text = "Generate the requirement ledger to inspect inbound links, child requirement chains, and active ext-data fields.";

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 520 };
            _requirementsGrid = CreateReadOnlyGrid();
            _requirementsGrid.AutoGenerateColumns = false;
            _requirementsGrid.Columns.Add(CreateTextColumn("RequirementId", "RequirementId", 95));
            _requirementsGrid.Columns.Add(CreateTextColumn("Rows", "RecordCount", 60));
            _requirementsGrid.Columns.Add(CreateTextColumn("Fields", "FieldCount", 60));
            _requirementsGrid.Columns.Add(CreateTextColumn("Abilities", "DirectAbilityCount", 70));
            _requirementsGrid.Columns.Add(CreateTextColumn("Components", "DirectComponentCount", 80));
            _requirementsGrid.Columns.Add(CreateTextColumn("Parents", "ParentRequirementCount", 70));
            _requirementsGrid.Columns.Add(CreateTextColumn("Children", "ChildRequirementCount", 70));
            _requirementsGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 160));
            _requirementsGrid.Columns.Add(CreateTextColumn("Summary", "SemanticSummary", 420));
            _requirementsGrid.SelectionChanged += (sender, args) => RefreshRequirementDetails();
            split.Panel1.Controls.Add(WrapInGroup("Requirement Summary", _requirementsGrid));

            TabControl details = new TabControl { Dock = DockStyle.Fill };
            _requirementRowGrid = CreateReadOnlyGrid();
            _requirementRowGrid.AutoGenerateColumns = false;
            _requirementRowGrid.Columns.Add(CreateTextColumn("RecordIndex", "RecordIndex", 90));
            _requirementRowGrid.Columns.Add(CreateTextColumn("ExtData", "ExtDataText", 520));
            _requirementRowGrid.Columns.Add(CreateTextColumn("Path", "SourcePath", 300));
            _requirementRowGrid.Columns.Add(CreateTextColumn("Location", "SourceLocation", 90));
            TabPage rowsTab = new TabPage("Rows");
            rowsTab.Controls.Add(_requirementRowGrid);
            details.TabPages.Add(rowsTab);

            _requirementFieldGrid = CreateReadOnlyGrid();
            _requirementFieldGrid.AutoGenerateColumns = false;
            _requirementFieldGrid.Columns.Add(CreateTextColumn("Field", "FieldKey", 160));
            _requirementFieldGrid.Columns.Add(CreateTextColumn("NonZero", "NonZeroCount", 70));
            _requirementFieldGrid.Columns.Add(CreateTextColumn("Distinct", "DistinctValueCount", 70));
            _requirementFieldGrid.Columns.Add(CreateTextColumn("SampleValues", "SampleValuesText", 220));
            _requirementFieldGrid.Columns.Add(CreateTextColumn("Meaning", "SemanticSummary", 260));
            _requirementFieldGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 90));
            _requirementFieldGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 260));
            TabPage fieldsTab = new TabPage("Fields");
            fieldsTab.Controls.Add(_requirementFieldGrid);
            details.TabPages.Add(fieldsTab);

            _requirementReferenceGrid = CreateReadOnlyGrid();
            _requirementReferenceGrid.AutoGenerateColumns = false;
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("Direction", "Direction", 80));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("SourceKind", "SourceKind", 90));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("SourceId", "SourceId", 85));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("SourceLabel", "SourceLabel", 220));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("Field", "SourceField", 130));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("LinkedRequirement", "LinkedRequirementId", 95));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("Abilities", "RelatedAbilitiesText", 220));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 160));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("Path", "SourcePath", 260));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("Location", "SourceLocation", 90));
            _requirementReferenceGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 260));
            TabPage linksTab = new TabPage("Links");
            linksTab.Controls.Add(_requirementReferenceGrid);
            details.TabPages.Add(linksTab);

            split.Panel2.Controls.Add(details);

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_requirementsSummaryTextBox, 0, 1);
            layout.Controls.Add(split, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateOperationTab()
        {
            TabPage tab = new TabPage("Operation Schemas");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateOperationSchemasButton = new Button { Text = "Generate Operation Schemas", AutoSize = true, Enabled = false };
            _generateOperationSchemasButton.Click += async (sender, args) => await GenerateOperationSchemaReportAsync();
            actions.Controls.Add(_generateOperationSchemasButton);
            _openOperationSchemasMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openOperationSchemasMarkdownButton.Click += (sender, args) => OpenPath(_lastOperationSchemaMarkdownPath);
            actions.Controls.Add(_openOperationSchemasMarkdownButton);
            _openOperationSchemasFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openOperationSchemasFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastOperationSchemaMarkdownPath) ? null : Path.GetDirectoryName(_lastOperationSchemaMarkdownPath));
            actions.Controls.Add(_openOperationSchemasFolderButton);

            _operationSummaryTextBox = CreateReadOnlyTextBox();
            _operationSummaryTextBox.Text = "Generate operation schemas to inspect component operations, their non-zero fields, semantic hints, and sample abilities.";

            SplitContainer outerSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 460 };
            _operationGrid = CreateReadOnlyGrid();
            _operationGrid.AutoGenerateColumns = false;
            _operationGrid.Columns.Add(CreateTextColumn("OperationId", "OperationId", 90));
            _operationGrid.Columns.Add(CreateTextColumn("OperationName", "OperationName", 180));
            _operationGrid.Columns.Add(CreateTextColumn("Priority", "PriorityText", 80));
            _operationGrid.Columns.Add(CreateTextColumn("Components", "ComponentCount", 90));
            _operationGrid.Columns.Add(CreateTextColumn("Abilities", "AbilityCount", 90));
            _operationGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 180));
            _operationGrid.SelectionChanged += (sender, args) => RefreshOperationDetails();
            outerSplit.Panel1.Controls.Add(WrapInGroup("Operations", _operationGrid));

            SplitContainer innerSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 290 };
            _operationFieldGrid = CreateReadOnlyGrid();
            _operationFieldGrid.AutoGenerateColumns = false;
            _operationFieldGrid.Columns.Add(CreateTextColumn("Field", "FieldKey", 170));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Triage", "TriageBucket", 80));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Score", "TriageScore", 65));
            _operationFieldGrid.Columns.Add(CreateTextColumn("NonZero", "NonZeroCount", 70));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Distinct", "DistinctValueCount", 70));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Sample Values", "SampleValuesText", 220));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Tokens", "TokenRenderingsText", 220));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Meaning", "SemanticSummary", 360));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 90));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 160));
            _operationFieldGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 260));
            innerSplit.Panel1.Controls.Add(WrapInGroup("Field Schema", _operationFieldGrid));

            _operationAbilityGrid = CreateReadOnlyGrid();
            _operationAbilityGrid.AutoGenerateColumns = false;
            _operationAbilityGrid.Columns.Add(CreateTextColumn("AbilityId", "AbilityId", 90));
            _operationAbilityGrid.Columns.Add(CreateTextColumn("AbilityName", "AbilityName", 170));
            _operationAbilityGrid.Columns.Add(CreateTextColumn("ComponentId", "ComponentId", 95));
            _operationAbilityGrid.Columns.Add(CreateTextColumn("Slot", "ComponentSlotIndex", 60));
            _operationAbilityGrid.Columns.Add(CreateTextColumn("Trigger", "TriggerText", 170));
            _operationAbilityGrid.Columns.Add(CreateTextColumn("Tags", "ContextTagsText", 150));
            _operationAbilityGrid.Columns.Add(CreateTextColumn("TextExcerpt", "TextExcerpt", 420));
            _operationAbilityGrid.Columns.Add(CreateTextColumn("Source", "SourceLocation", 90));
            innerSplit.Panel2.Controls.Add(WrapInGroup("Sample Abilities", _operationAbilityGrid));

            outerSplit.Panel2.Controls.Add(innerSplit);

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_operationSummaryTextBox, 0, 1);
            layout.Controls.Add(outerSplit, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateCoverageTab()
        {
            TabPage tab = new TabPage("Coverage");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateCoverageButton = new Button { Text = "Generate Coverage Report", AutoSize = true, Enabled = false };
            _generateCoverageButton.Click += async (sender, args) => await GenerateCoverageReportAsync();
            actions.Controls.Add(_generateCoverageButton);
            _openCoverageMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openCoverageMarkdownButton.Click += (sender, args) => OpenPath(_lastCoverageMarkdownPath);
            actions.Controls.Add(_openCoverageMarkdownButton);
            _openCoverageFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openCoverageFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastCoverageMarkdownPath) ? null : Path.GetDirectoryName(_lastCoverageMarkdownPath));
            actions.Controls.Add(_openCoverageFolderButton);

            _coverageSummaryTextBox = CreateReadOnlyTextBox();
            _coverageSummaryTextBox.Text = "Generate the coverage report to see which abilities are fully mapped and which still have extracted-client gaps.";

            _coverageGrid = CreateReadOnlyGrid();
            _coverageGrid.AutoGenerateColumns = false;
            _coverageGrid.Columns.Add(CreateTextColumn("AbilityId", "AbilityId", 90));
            _coverageGrid.Columns.Add(CreateTextColumn("Name", "Name", 210));
            _coverageGrid.Columns.Add(CreateTextColumn("Status", "CoverageStatus", 140));
            _coverageGrid.Columns.Add(CreateTextColumn("EffectId", "EffectIdText", 80));
            _coverageGrid.Columns.Add(CreateTextColumn("Csv", "HasClientCsvText", 55));
            _coverageGrid.Columns.Add(CreateTextColumn("Bin", "HasClientBinText", 55));
            _coverageGrid.Columns.Add(CreateTextColumn("NameText", "HasLocalizedNameText", 75));
            _coverageGrid.Columns.Add(CreateTextColumn("DescText", "HasDescriptionTextText", 75));
            _coverageGrid.Columns.Add(CreateTextColumn("EffectText", "HasEffectTextText", 75));
            _coverageGrid.Columns.Add(CreateTextColumn("EffectRow", "HasRootEffectRowText", 75));
            _coverageGrid.Columns.Add(CreateTextColumn("Components", "ComponentCount", 85));
            _coverageGrid.Columns.Add(CreateTextColumn("ReqLinks", "RequirementLinkCount", 75));
            _coverageGrid.Columns.Add(CreateTextColumn("ReqRows", "RequirementRowCount", 70));
            _coverageGrid.Columns.Add(CreateTextColumn("Sources", "SourcesText", 170));
            _coverageGrid.Columns.Add(CreateTextColumn("Missing", "MissingText", 220));

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_coverageSummaryTextBox, 0, 1);
            layout.Controls.Add(_coverageGrid, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateRemainingWorkTab()
        {
            TabPage tab = new TabPage("Remaining Work");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _generateRemainingWorkButton = new Button { Text = "Generate Remaining Work", AutoSize = true, Enabled = false };
            _generateRemainingWorkButton.Click += async (sender, args) => await GenerateRemainingWorkReportAsync();
            actions.Controls.Add(_generateRemainingWorkButton);
            _openRemainingWorkMarkdownButton = new Button { Text = "Open Markdown", AutoSize = true, Enabled = false };
            _openRemainingWorkMarkdownButton.Click += (sender, args) => OpenPath(_lastRemainingWorkMarkdownPath);
            actions.Controls.Add(_openRemainingWorkMarkdownButton);
            _openRemainingWorkNextMarkdownButton = new Button { Text = "Open Next Batch", AutoSize = true, Enabled = false };
            _openRemainingWorkNextMarkdownButton.Click += (sender, args) => OpenPath(_lastRemainingWorkNextMarkdownPath);
            actions.Controls.Add(_openRemainingWorkNextMarkdownButton);
            _openRemainingWorkOperationFieldsMarkdownButton = new Button { Text = "Open Field Packets", AutoSize = true, Enabled = false };
            _openRemainingWorkOperationFieldsMarkdownButton.Click += (sender, args) => OpenPath(_lastRemainingWorkOperationFieldsMarkdownPath);
            actions.Controls.Add(_openRemainingWorkOperationFieldsMarkdownButton);
            _openRemainingWorkLiteralCrosswalkMarkdownButton = new Button { Text = "Open Literal Crosswalk", AutoSize = true, Enabled = false };
            _openRemainingWorkLiteralCrosswalkMarkdownButton.Click += (sender, args) => OpenPath(_lastRemainingWorkLiteralCrosswalkMarkdownPath);
            actions.Controls.Add(_openRemainingWorkLiteralCrosswalkMarkdownButton);
            _openRemainingWorkFolderButton = new Button { Text = "Open Output Folder", AutoSize = true, Enabled = false };
            _openRemainingWorkFolderButton.Click += (sender, args) => OpenPath(string.IsNullOrWhiteSpace(_lastRemainingWorkMarkdownPath) ? null : Path.GetDirectoryName(_lastRemainingWorkMarkdownPath));
            actions.Controls.Add(_openRemainingWorkFolderButton);
            _remainingWorkAllAreasCheckBox = new CheckBox { Text = "All Areas", AutoSize = true };
            _remainingWorkAllAreasCheckBox.CheckedChanged += (sender, args) => RefreshRemainingWorkItems();
            actions.Controls.Add(_remainingWorkAllAreasCheckBox);
            actions.Controls.Add(new Label { Text = "Priority", AutoSize = true, Margin = new Padding(10, 8, 4, 0) });
            _remainingWorkPriorityFilterComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            _remainingWorkPriorityFilterComboBox.Items.AddRange(new object[] { "All", "Critical", "High", "Medium", "Low" });
            _remainingWorkPriorityFilterComboBox.SelectedIndex = 0;
            _remainingWorkPriorityFilterComboBox.SelectedIndexChanged += (sender, args) => RefreshRemainingWorkItems();
            actions.Controls.Add(_remainingWorkPriorityFilterComboBox);
            actions.Controls.Add(new Label { Text = "Search", AutoSize = true, Margin = new Padding(10, 8, 4, 0) });
            _remainingWorkSearchTextBox = new TextBox { Width = 180 };
            _remainingWorkSearchTextBox.TextChanged += (sender, args) => RefreshRemainingWorkItems();
            actions.Controls.Add(_remainingWorkSearchTextBox);
            actions.Controls.Add(new Label { Text = "Top (0=all)", AutoSize = true, Margin = new Padding(10, 8, 4, 0) });
            _remainingWorkTopCountUpDown = new NumericUpDown { Minimum = 0, Maximum = 5000, Value = 0, Width = 70 };
            _remainingWorkTopCountUpDown.ValueChanged += (sender, args) => RefreshRemainingWorkItems();
            actions.Controls.Add(_remainingWorkTopCountUpDown);
            Button resetRemainingFiltersButton = new Button { Text = "Reset Filters", AutoSize = true };
            resetRemainingFiltersButton.Click += (sender, args) =>
            {
                _remainingWorkAllAreasCheckBox.Checked = false;
                _remainingWorkPriorityFilterComboBox.SelectedIndex = 0;
                _remainingWorkSearchTextBox.Text = string.Empty;
                _remainingWorkTopCountUpDown.Value = 0;
            };
            actions.Controls.Add(resetRemainingFiltersButton);

            _remainingWorkSummaryTextBox = CreateReadOnlyTextBox();
            _remainingWorkSummaryTextBox.Text = "Generate the remaining-work report to rank the outstanding matrix backlog across coverage, conflicts, unknown fields, requirements, tokens, and identity domains.";

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 360 };
            _remainingWorkAreaGrid = CreateReadOnlyGrid();
            _remainingWorkAreaGrid.AutoGenerateColumns = false;
            _remainingWorkAreaGrid.Columns.Add(CreateTextColumn("Area", "Title", 170));
            _remainingWorkAreaGrid.Columns.Add(CreateTextColumn("Peak", "PeakScore", 70));
            _remainingWorkAreaGrid.Columns.Add(CreateTextColumn("Bucket", "PeakBucket", 80));
            _remainingWorkAreaGrid.Columns.Add(CreateTextColumn("Critical", "CriticalCount", 70));
            _remainingWorkAreaGrid.Columns.Add(CreateTextColumn("High", "HighCount", 70));
            _remainingWorkAreaGrid.Columns.Add(CreateTextColumn("Items", "ItemCount", 70));
            _remainingWorkAreaGrid.SelectionChanged += (sender, args) => RefreshRemainingWorkItems();
            split.Panel1.Controls.Add(WrapInGroup("Backlog Areas", _remainingWorkAreaGrid));

            SplitContainer detailSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 360 };
            _remainingWorkItemGrid = CreateReadOnlyGrid();
            _remainingWorkItemGrid.AutoGenerateColumns = false;
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Rank", "Rank", 55));
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Global", "GlobalRank", 55));
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Priority", "PriorityBucket", 75));
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Score", "PriorityScore", 60));
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Title", "Title", 240));
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Subject", "SubjectKey", 190));
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Ability", "ExampleAbilityIdText", 70));
            _remainingWorkItemGrid.Columns.Add(CreateTextColumn("Summary", "Summary", 420));
            _remainingWorkItemGrid.SelectionChanged += (sender, args) => RefreshRemainingWorkDetail();
            _remainingWorkItemGrid.CellDoubleClick += async (sender, args) => { if (args.RowIndex >= 0) await NavigateSelectedRemainingWorkAbilityAsync(); };
            detailSplit.Panel1.Controls.Add(WrapInGroup("Backlog Items", _remainingWorkItemGrid));

            _remainingWorkDetailTextBox = CreateReadOnlyTextBox();
            _remainingWorkDetailTextBox.Text = "Select a remaining-work item to inspect its summary, evidence, and suggested next action.";
            detailSplit.Panel2.Controls.Add(WrapInGroup("Item Detail", _remainingWorkDetailTextBox));

            split.Panel2.Controls.Add(detailSplit);

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(_remainingWorkSummaryTextBox, 0, 1);
            layout.Controls.Add(split, 0, 2);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateStatusTab()
        {
            TabPage tab = new TabPage("Source Status");
            _statusGrid = CreateReadOnlyGrid();
            _statusGrid.AutoGenerateColumns = false;
            _statusGrid.Columns.Add(CreateTextColumn("Source", "SourceFamily", 110));
            _statusGrid.Columns.Add(CreateTextColumn("File", "TableName", 180));
            _statusGrid.Columns.Add(CreateTextColumn("Loaded", "Loaded", 70));
            _statusGrid.Columns.Add(CreateTextColumn("Rows", "RowCount", 90));
            _statusGrid.Columns.Add(CreateTextColumn("Path", "SourcePath", 500));
            _statusGrid.Columns.Add(CreateTextColumn("Error", "ErrorMessage", 320));
            tab.Controls.Add(_statusGrid);
            return tab;
        }

        private TabPage CreateLogTab()
        {
            TabPage tab = new TabPage("Log");
            _logTextBox = CreateReadOnlyTextBox();
            _logTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            _logTextBox.Text = "Session log will appear here." + Environment.NewLine;
            tab.Controls.Add(_logTextBox);
            return tab;
        }

        private async Task LoadSessionAsync()
        {
            if (_isBusy)
                return;

            // Point the client-reading tabs at whatever the boxes now hold, so changing a root and
            // pressing Reload Data does not leave them reading the previous extraction.
            if (_itemArtTab != null)
                _itemArtTab.Bind(_rootPathTextBox.Text);
            if (_crosswalkTab != null)
                _crosswalkTab.Bind(_rootPathTextBox.Text, _outputPathTextBox.Text);
            if (_clientSearchTab != null)
                _clientSearchTab.Bind(_rootPathTextBox.Text, _outputPathTextBox.Text);

            try
            {
                SetBusy(true, "Loading extracted client dataset...");
                _session = await Task.Run(() => MatrixAnalysisSession.Load(_rootPathTextBox.Text.Trim()));
                _definitions = new DefinitionCatalog(_session.Dataset);
                _narrativeBuilder = new AbilityNarrativeBuilder(_definitions);
                _abilityCatalog = _session.BuildAbilityCatalog();
                _lastAbilityReport = null;
                _lastTokenDictionary = null;
                _lastCoverageReport = null;
                _lastRemainingWorkReport = null;
                _lastDomainLedger = null;
                _lastRequirementLedger = null;
                _lastOperationSchema = null;
                _lastConflictReport = null;
                _lastAbilityMarkdownPath = null;
                _lastTokenDictionaryMarkdownPath = null;
                _lastCoverageMarkdownPath = null;
                _lastRemainingWorkMarkdownPath = null;
                _lastRemainingWorkNextMarkdownPath = null;
                _lastRemainingWorkOperationFieldsMarkdownPath = null;
                _lastRemainingWorkLiteralCrosswalkMarkdownPath = null;
                _lastDomainLedgerMarkdownPath = null;
                _lastRequirementMarkdownPath = null;
                _lastOperationSchemaMarkdownPath = null;
                _lastConflictMarkdownPath = null;
                _unknownValueCache.Clear();
                _unknownInsightCache.Clear();
                _openAbilityMarkdownButton.Enabled = false;
                _openAbilityFolderButton.Enabled = false;
                _openTokenDictionaryMarkdownButton.Enabled = false;
                _openTokenDictionaryFolderButton.Enabled = false;
                _openCoverageMarkdownButton.Enabled = false;
                _openCoverageFolderButton.Enabled = false;
                _openRemainingWorkMarkdownButton.Enabled = false;
                _openRemainingWorkNextMarkdownButton.Enabled = false;
                _openRemainingWorkOperationFieldsMarkdownButton.Enabled = false;
                _openRemainingWorkLiteralCrosswalkMarkdownButton.Enabled = false;
                _openRemainingWorkFolderButton.Enabled = false;
                _openDomainsMarkdownButton.Enabled = false;
                _openDomainsFolderButton.Enabled = false;
                _openRequirementsMarkdownButton.Enabled = false;
                _openRequirementsFolderButton.Enabled = false;
                _openOperationSchemasMarkdownButton.Enabled = false;
                _openOperationSchemasFolderButton.Enabled = false;
                _openUnknownMarkdownButton.Enabled = false;
                _openUnknownFolderButton.Enabled = false;
                _hideMultiplierUnknownsCheckBox.Checked = true;
                _openConflictMarkdownButton.Enabled = false;
                _openConflictFolderButton.Enabled = false;
                _hideBlankStringConflictCheckBox.Checked = false;
                _hideMirrorEffectIdConflictCheckBox.Checked = true;
                _highSignalConflictsOnlyCheckBox.Checked = false;

                _statusGrid.DataSource = _session.Dataset.TableStatuses.OrderBy(x => x.SourceFamily).ThenBy(x => x.TableName).ToList();
                ApplyAbilityFilter();
                ClearAbilityPresentation(BuildDatasetSummary());
                _tokenSummaryTextBox.Text = "Generate the COM token dictionary to write plain-English token definitions from extracted client evidence.";
                _tokenGrid.DataSource = null;
                _domainsSummaryTextBox.Text = "Generate the core identity domain ledger to separate proven extracted-client domains such as Race.EntryId, CareerLine.EntryId, and repeated CareerName entry ids.";
                _domainsGrid.DataSource = null;
                _domainValuesGrid.DataSource = null;
                _requirementsSummaryTextBox.Text = "Generate the requirement ledger to inspect inbound links, child requirement chains, and active ext-data fields.";
                _requirementsGrid.DataSource = null;
                _requirementRowGrid.DataSource = null;
                _requirementFieldGrid.DataSource = null;
                _requirementReferenceGrid.DataSource = null;
                _coverageSummaryTextBox.Text = "Generate the coverage report to see which abilities are fully mapped and which still have extracted-client gaps.";
                _coverageGrid.DataSource = null;
                _remainingWorkSummaryTextBox.Text = "Generate the remaining-work report to rank the outstanding matrix backlog across coverage, conflicts, unknown fields, requirements, tokens, and identity domains.";
                _remainingWorkAreaGrid.DataSource = null;
                _remainingWorkItemGrid.DataSource = null;
                _remainingWorkDetailTextBox.Text = "Select a remaining-work item to inspect its summary, evidence, and suggested next action.";
                _operationSummaryTextBox.Text = "Generate operation schemas to inspect component operations, their non-zero fields, semantic hints, and sample abilities.";
                _operationGrid.DataSource = null;
                _operationFieldGrid.DataSource = null;
                _operationAbilityGrid.DataSource = null;
                _unknownSummaryTextBox.Text = "Generate operation schemas to rank the remaining unknown component fields by impact and priority.";
                _unknownFieldGrid.DataSource = null;
                _unknownValueGrid.DataSource = null;
                _unknownCompanionGrid.DataSource = null;
                _unknownAbilityGrid.DataSource = null;
                _unknownInsightTextBox.Text = "Select an unknown field and raw value to inspect its trigger mix, context tags, and dominant companion fields.";
                _conflictSummaryTextBox.Text = "Generate the conflict ledger to browse contradiction domains.";
                _conflictDomainGrid.DataSource = null;
                _conflictGrid.DataSource = null;
                _conflictValueGrid.DataSource = null;
                _conflictClaimGrid.DataSource = null;
                _conflictInsightTextBox.Text = "Select a conflict to inspect its subject, decoded values, and source pattern.";

                AppendLog("Loaded dataset from " + _session.ExtractedRootPath + ".");
                AppendLog("Catalog contains " + _abilityCatalog.Count.ToString(CultureInfo.InvariantCulture) + " unique ability ids.");
                SetBusy(false, "Dataset loaded.");
            }
            catch (Exception ex)
            {
                _session = null;
                _definitions = null;
                _narrativeBuilder = null;
                _abilityCatalog = new List<AbilityCatalogEntry>();
                _lastDomainLedger = null;
                _lastDomainLedgerMarkdownPath = null;
                _lastRequirementLedger = null;
                _lastRequirementMarkdownPath = null;
                _lastRemainingWorkReport = null;
                _lastRemainingWorkMarkdownPath = null;
                _lastRemainingWorkNextMarkdownPath = null;
                _lastRemainingWorkOperationFieldsMarkdownPath = null;
                _lastRemainingWorkLiteralCrosswalkMarkdownPath = null;
                _abilityGrid.DataSource = null;
                _statusGrid.DataSource = null;
                _abilityCatalogStatusLabel.Text = "Dataset load failed.";
                ClearAbilityPresentation(ex.ToString());
                _tokenGrid.DataSource = null;
                _openTokenDictionaryMarkdownButton.Enabled = false;
                _openTokenDictionaryFolderButton.Enabled = false;
                _domainsGrid.DataSource = null;
                _domainValuesGrid.DataSource = null;
                _domainsSummaryTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _openDomainsMarkdownButton.Enabled = false;
                _openDomainsFolderButton.Enabled = false;
                _requirementsGrid.DataSource = null;
                _requirementRowGrid.DataSource = null;
                _requirementFieldGrid.DataSource = null;
                _requirementReferenceGrid.DataSource = null;
                _requirementsSummaryTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _openRequirementsMarkdownButton.Enabled = false;
                _openRequirementsFolderButton.Enabled = false;
                _coverageGrid.DataSource = null;
                _coverageSummaryTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _openCoverageMarkdownButton.Enabled = false;
                _openCoverageFolderButton.Enabled = false;
                _remainingWorkAreaGrid.DataSource = null;
                _remainingWorkItemGrid.DataSource = null;
                _remainingWorkSummaryTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _remainingWorkDetailTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _openRemainingWorkMarkdownButton.Enabled = false;
                _openRemainingWorkNextMarkdownButton.Enabled = false;
                _openRemainingWorkOperationFieldsMarkdownButton.Enabled = false;
                _openRemainingWorkLiteralCrosswalkMarkdownButton.Enabled = false;
                _openRemainingWorkFolderButton.Enabled = false;
                _operationGrid.DataSource = null;
                _operationFieldGrid.DataSource = null;
                _operationAbilityGrid.DataSource = null;
                _operationSummaryTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _openOperationSchemasMarkdownButton.Enabled = false;
                _openOperationSchemasFolderButton.Enabled = false;
                _unknownValueCache.Clear();
                _unknownInsightCache.Clear();
                _unknownFieldGrid.DataSource = null;
                _unknownValueGrid.DataSource = null;
                _unknownCompanionGrid.DataSource = null;
                _unknownAbilityGrid.DataSource = null;
                _unknownSummaryTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _unknownInsightTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _openUnknownMarkdownButton.Enabled = false;
                _openUnknownFolderButton.Enabled = false;
                _hideMultiplierUnknownsCheckBox.Checked = true;
                _conflictSummaryTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _conflictDomainGrid.DataSource = null;
                _conflictGrid.DataSource = null;
                _conflictValueGrid.DataSource = null;
                _conflictClaimGrid.DataSource = null;
                _conflictInsightTextBox.Text = "Dataset load failed. Fix the extracted root path and reload.";
                _openConflictMarkdownButton.Enabled = false;
                _openConflictFolderButton.Enabled = false;
                _hideBlankStringConflictCheckBox.Checked = false;
                _hideMirrorEffectIdConflictCheckBox.Checked = true;
                _highSignalConflictsOnlyCheckBox.Checked = false;
                AppendLog("Dataset load failed: " + ex.Message);
                SetBusy(false, "Dataset load failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> GenerateAbilityReportAsync()
        {
            if (_isBusy || _session == null)
                return false;

            ushort abilityId;
            if (!TryGetSelectedAbilityId(out abilityId))
            {
                MessageBox.Show(this, "Ability ID must be a valid unsigned 16-bit integer.", "ClientDataMatrix", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                SetBusy(true, "Generating ability report for " + abilityId.ToString(CultureInfo.InvariantCulture) + "...");
                string outputRoot = ResolveOutputRoot();
                AbilityAnalysisResult report = await Task.Run(() => _session.BuildAbilityAnalysis(abilityId));
                _lastAbilityMarkdownPath = _session.WriteAbilityReport(outputRoot, report);
                _lastAbilityReport = report;
                _openAbilityMarkdownButton.Enabled = File.Exists(_lastAbilityMarkdownPath);
                _openAbilityFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastAbilityMarkdownPath));
                PopulateAbilityPresentation(report, _lastAbilityMarkdownPath);
                AppendLog("Ability " + abilityId.ToString(CultureInfo.InvariantCulture) + " report written to " + _lastAbilityMarkdownPath + ".");
                SetBusy(false, "Ability report generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Ability report failed: " + ex.Message);
                SetBusy(false, "Ability report failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Ability Report Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> GenerateConflictReportAsync()
        {
            if (_isBusy || _session == null)
                return false;

            try
            {
                SetBusy(true, "Generating conflict report...");
                string outputRoot = ResolveOutputRoot();
                ConflictReportDocument report = await Task.Run(() => _session.BuildConflictReport());
                _lastConflictMarkdownPath = _session.WriteConflictReport(outputRoot, report);
                _lastConflictReport = report;
                _openConflictMarkdownButton.Enabled = File.Exists(_lastConflictMarkdownPath);
                _openConflictFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastConflictMarkdownPath));
                RefreshConflictView();
                AppendLog("Conflict report written to " + _lastConflictMarkdownPath + ".");
                SetBusy(false, "Conflict report generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Conflict report failed: " + ex.Message);
                SetBusy(false, "Conflict report failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Conflict Report Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> GenerateTokenDictionaryAsync()
        {
            if (_isBusy || _session == null)
                return false;

            try
            {
                SetBusy(true, "Generating token dictionary...");
                string outputRoot = ResolveOutputRoot();
                TokenDictionaryDocument report = await Task.Run(() => _session.BuildTokenDictionary());
                _lastTokenDictionaryMarkdownPath = await Task.Run(() => _session.WriteTokenDictionaryReport(outputRoot, report));
                _lastTokenDictionary = report;
                _openTokenDictionaryMarkdownButton.Enabled = File.Exists(_lastTokenDictionaryMarkdownPath);
                _openTokenDictionaryFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastTokenDictionaryMarkdownPath));
                _tokenSummaryTextBox.Text = BuildTokenDictionarySummary(report, _lastTokenDictionaryMarkdownPath);
                _tokenGrid.DataSource = report.Definitions == null
                    ? new List<TokenDefinitionListRow>()
                    : report.Definitions.Select(row => new TokenDefinitionListRow
                    {
                        ExampleToken = row.ExampleToken,
                        FieldKey = row.FieldKey,
                        PlainEnglishMeaning = row.PlainEnglishMeaning,
                        Confidence = row.Confidence,
                        ContextTagsText = row.ContextTags == null ? string.Empty : string.Join(", ", row.ContextTags),
                        ExampleAbilityIdsText = row.ExampleAbilityIds == null ? string.Empty : string.Join(", ", row.ExampleAbilityIds.Select(value => value.ToString(CultureInfo.InvariantCulture))),
                        Notes = row.Notes
                    }).ToList();
                AppendLog("Token dictionary written to " + _lastTokenDictionaryMarkdownPath + ".");
                SetBusy(false, "Token dictionary generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Token dictionary failed: " + ex.Message);
                SetBusy(false, "Token dictionary failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Token Dictionary Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> GenerateOperationSchemaReportAsync()
        {
            if (_isBusy || _session == null)
                return false;

            try
            {
                SetBusy(true, "Generating operation schemas...");
                string outputRoot = ResolveOutputRoot();
                OperationSchemaDocument report = await Task.Run(() => _session.BuildOperationSchemas());
                _lastOperationSchemaMarkdownPath = await Task.Run(() => _session.WriteOperationSchemaReport(outputRoot, report));
                _lastOperationSchema = report;
                _unknownValueCache.Clear();
                _unknownInsightCache.Clear();
                _openOperationSchemasMarkdownButton.Enabled = File.Exists(_lastOperationSchemaMarkdownPath);
                _openOperationSchemasFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastOperationSchemaMarkdownPath));
                _openUnknownMarkdownButton.Enabled = _openOperationSchemasMarkdownButton.Enabled;
                _openUnknownFolderButton.Enabled = _openOperationSchemasFolderButton.Enabled;
                _operationSummaryTextBox.Text = BuildOperationSummary(report, _lastOperationSchemaMarkdownPath);
                _operationGrid.DataSource = report.Operations == null
                    ? new List<OperationSchemaListRow>()
                    : report.Operations.Select(row => new OperationSchemaListRow
                    {
                        OperationId = row.OperationId,
                        OperationName = row.OperationName,
                        PriorityText = row.IsPriority ? "Yes" : "No",
                        ComponentCount = row.ComponentCount,
                        AbilityCount = row.AbilityCount,
                        ContextTagsText = row.ContextTags == null ? string.Empty : string.Join(", ", row.ContextTags),
                        LayoutVariantsText = row.LayoutVariantsText
                    }).ToList();
                SelectFirstRow(_operationGrid);
                RefreshOperationDetails();
                RefreshUnknownView();
                AppendLog("Operation schema report written to " + _lastOperationSchemaMarkdownPath + ".");
                SetBusy(false, "Operation schemas generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Operation schema report failed: " + ex.Message);
                SetBusy(false, "Operation schema report failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Operation Schemas Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> GenerateDomainLedgerAsync()
        {
            if (_isBusy || _session == null)
                return false;

            try
            {
                SetBusy(true, "Generating core identity domains...");
                string outputRoot = ResolveOutputRoot();
                DomainLedgerDocument report = await Task.Run(() => _session.BuildDomainLedger());
                _lastDomainLedgerMarkdownPath = await Task.Run(() => _session.WriteDomainLedgerReport(outputRoot, report));
                _lastDomainLedger = report;
                _openDomainsMarkdownButton.Enabled = File.Exists(_lastDomainLedgerMarkdownPath);
                _openDomainsFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastDomainLedgerMarkdownPath));
                _domainsSummaryTextBox.Text = BuildDomainSummary(report, _lastDomainLedgerMarkdownPath);
                _domainsGrid.DataSource = report.Domains == null
                    ? new List<DomainListRow>()
                    : report.Domains.Select(row => new DomainListRow
                    {
                        DomainKey = row.DomainKey,
                        DisplayName = row.DisplayName,
                        Confidence = row.Confidence,
                        Canonicality = row.Canonicality,
                        ValueCount = row.ValueCount,
                        DistinctMeaningCount = row.DistinctMeaningCount,
                        DuplicateMeaningCount = row.DuplicateMeaningCount,
                        SourceFilesText = row.SourceFilesText,
                        RecommendedUsage = row.RecommendedUsage,
                        Notes = row.Notes
                    }).ToList();
                SelectFirstRow(_domainsGrid);
                RefreshDomainValues();
                AppendLog("Domain ledger written to " + _lastDomainLedgerMarkdownPath + ".");
                SetBusy(false, "Core identity domains generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Domain ledger failed: " + ex.Message);
                SetBusy(false, "Core identity domains failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Domain Ledger Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> GenerateRequirementLedgerAsync()
        {
            if (_isBusy || _session == null)
                return false;

            try
            {
                SetBusy(true, "Generating requirement ledger...");
                string outputRoot = ResolveOutputRoot();
                RequirementLedgerDocument report = await Task.Run(() => _session.BuildRequirementLedger());
                _lastRequirementMarkdownPath = await Task.Run(() => _session.WriteRequirementLedgerReport(outputRoot, report));
                _lastRequirementLedger = report;
                _openRequirementsMarkdownButton.Enabled = File.Exists(_lastRequirementMarkdownPath);
                _openRequirementsFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastRequirementMarkdownPath));
                _requirementsSummaryTextBox.Text = BuildRequirementSummary(report, _lastRequirementMarkdownPath);
                _requirementsGrid.DataSource = report.Requirements == null
                    ? new List<RequirementListRow>()
                    : report.Requirements.Select(row => new RequirementListRow
                    {
                        RequirementId = row.RequirementId,
                        RecordCount = row.RecordCount,
                        FieldCount = row.FieldCount,
                        DirectAbilityCount = row.DirectAbilityCount,
                        DirectComponentCount = row.DirectComponentCount,
                        ParentRequirementCount = row.ParentRequirementCount,
                        ChildRequirementCount = row.ChildRequirementCount,
                        ContextTagsText = row.ContextTagsText,
                        SampleAbilitiesText = row.SampleAbilitiesText,
                        SemanticSummary = row.SemanticSummary,
                        Notes = row.Notes
                    }).ToList();
                SelectFirstRow(_requirementsGrid);
                RefreshRequirementDetails();
                AppendLog("Requirement ledger written to " + _lastRequirementMarkdownPath + ".");
                SetBusy(false, "Requirement ledger generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Requirement ledger failed: " + ex.Message);
                SetBusy(false, "Requirement ledger failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Requirement Ledger Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> GenerateCoverageReportAsync()
        {
            if (_isBusy || _session == null)
                return false;

            try
            {
                SetBusy(true, "Generating coverage report...");
                string outputRoot = ResolveOutputRoot();
                CoverageReportDocument report = await Task.Run(() => _session.BuildCoverageReport());
                _lastCoverageMarkdownPath = await Task.Run(() => _session.WriteCoverageReport(outputRoot, report));
                _lastCoverageReport = report;
                _openCoverageMarkdownButton.Enabled = File.Exists(_lastCoverageMarkdownPath);
                _openCoverageFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastCoverageMarkdownPath));
                _coverageSummaryTextBox.Text = BuildCoverageSummary(report, _lastCoverageMarkdownPath);
                _coverageGrid.DataSource = report.Abilities == null
                    ? new List<CoverageListRow>()
                    : report.Abilities.Select(row => new CoverageListRow
                    {
                        AbilityId = row.AbilityId,
                        Name = row.Name,
                        CoverageStatus = row.CoverageStatus,
                        EffectIdText = row.EffectId.HasValue ? row.EffectId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                        HasClientCsvText = row.HasClientCsv ? "Yes" : "No",
                        HasClientBinText = row.HasClientBin ? "Yes" : "No",
                        HasLocalizedNameText = row.HasLocalizedName ? "Yes" : "No",
                        HasDescriptionTextText = row.HasDescriptionText ? "Yes" : "No",
                        HasEffectTextText = row.HasEffectText ? "Yes" : "No",
                        HasRootEffectRowText = row.HasRootEffectRow ? "Yes" : "No",
                        ComponentCount = row.ComponentCount,
                        RequirementLinkCount = row.RequirementLinkCount,
                        RequirementRowCount = row.RequirementRowCount,
                        SourcesText = row.SourcesText,
                        MissingText = row.MissingText
                    }).ToList();
                AppendLog("Coverage report written to " + _lastCoverageMarkdownPath + ".");
                SetBusy(false, "Coverage report generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Coverage report failed: " + ex.Message);
                SetBusy(false, "Coverage report failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Coverage Report Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> GenerateRemainingWorkReportAsync()
        {
            if (_isBusy || _session == null)
                return false;

            try
            {
                SetBusy(true, "Generating remaining-work report...");
                string outputRoot = ResolveOutputRoot();
                CoverageReportDocument coverage = _lastCoverageReport ?? await Task.Run(() => _session.BuildCoverageReport());
                ConflictReportDocument conflicts = _lastConflictReport ?? await Task.Run(() => _session.BuildConflictReport());
                DomainLedgerDocument domains = _lastDomainLedger ?? await Task.Run(() => _session.BuildDomainLedger());
                RequirementLedgerDocument requirements = _lastRequirementLedger ?? await Task.Run(() => _session.BuildRequirementLedger());
                TokenDictionaryDocument tokens = _lastTokenDictionary ?? await Task.Run(() => _session.BuildTokenDictionary());
                OperationSchemaDocument operations = _lastOperationSchema ?? await Task.Run(() => _session.BuildOperationSchemas());
                RemainingWorkDocument report = await Task.Run(() => _session.BuildRemainingWorkReport(coverage, conflicts, domains, requirements, tokens, operations));
                OperationFieldWorkPacketDocument packetReport = await Task.Run(() => _session.BuildOperationFieldWorkPackets(
                    report,
                    operations,
                    OperationFieldWorkPacketCatalog.DefaultPacketCount,
                    RemainingWorkCatalog.DefaultNextBatchMinimumPriorityBucket,
                    null));
                ControlLiteralCrosswalkDocument literalReport = await Task.Run(() => _session.BuildControlLiteralCrosswalk(
                    requirements,
                    ControlLiteralCrosswalkCatalog.DefaultLiteralCount,
                    null));
                _lastRemainingWorkMarkdownPath = await Task.Run(() => _session.WriteRemainingWorkReport(outputRoot, report));
                _lastRemainingWorkNextMarkdownPath = Path.Combine(Path.GetDirectoryName(_lastRemainingWorkMarkdownPath), "remaining-work-next.md");
                _lastRemainingWorkOperationFieldsMarkdownPath = await Task.Run(() => _session.WriteOperationFieldWorkPacketReport(outputRoot, packetReport));
                _lastRemainingWorkLiteralCrosswalkMarkdownPath = await Task.Run(() => _session.WriteControlLiteralCrosswalkReport(outputRoot, literalReport));
                _lastRemainingWorkReport = report;
                _openRemainingWorkMarkdownButton.Enabled = File.Exists(_lastRemainingWorkMarkdownPath);
                _openRemainingWorkNextMarkdownButton.Enabled = File.Exists(_lastRemainingWorkNextMarkdownPath);
                _openRemainingWorkOperationFieldsMarkdownButton.Enabled = File.Exists(_lastRemainingWorkOperationFieldsMarkdownPath);
                _openRemainingWorkLiteralCrosswalkMarkdownButton.Enabled = File.Exists(_lastRemainingWorkLiteralCrosswalkMarkdownPath);
                _openRemainingWorkFolderButton.Enabled = Directory.Exists(Path.GetDirectoryName(_lastRemainingWorkMarkdownPath));
                _remainingWorkSummaryTextBox.Text = BuildRemainingWorkSummary(report, _lastRemainingWorkMarkdownPath);
                _remainingWorkAreaGrid.DataSource = report.Areas == null
                    ? new List<RemainingWorkAreaListRow>()
                    : report.Areas.Select(row => new RemainingWorkAreaListRow
                    {
                        AreaKey = row.AreaKey,
                        Title = row.Title,
                        ItemCount = row.ItemCount,
                        CriticalCount = row.CriticalCount,
                        HighCount = row.HighCount,
                        PeakScore = row.PeakScore,
                        PeakBucket = row.PeakBucket,
                        Summary = row.Summary,
                        RecommendedNextStep = row.RecommendedNextStep
                    }).ToList();
                SelectFirstRow(_remainingWorkAreaGrid);
                RefreshRemainingWorkItems();
                AppendLog("Remaining-work report written to " + _lastRemainingWorkMarkdownPath + ".");
                SetBusy(false, "Remaining-work report generated.");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog("Remaining-work report failed: " + ex.Message);
                SetBusy(false, "Remaining-work report failed.");
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Remaining Work Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task GenerateAllReportsAsync()
        {
            if (_isBusy || _session == null)
                return;

            AppendLog("Generate All started.");

            if (!await GenerateDomainLedgerAsync())
                return;
            if (!await GenerateRequirementLedgerAsync())
                return;
            if (!await GenerateTokenDictionaryAsync())
                return;
            if (!await GenerateCoverageReportAsync())
                return;
            if (!await GenerateOperationSchemaReportAsync())
                return;
            if (!await GenerateConflictReportAsync())
                return;
            if (!await GenerateRemainingWorkReportAsync())
                return;

            ushort abilityId;
            if (TryGetSelectedAbilityId(out abilityId))
            {
                if (!await GenerateAbilityReportAsync())
                    return;
            }
            else
                AppendLog("Generate All skipped ability report because no valid ability id is currently selected.");

            _statusLabel.Text = "Generate All finished.";
            AppendLog("Generate All completed.");
        }

        private bool TryGetSelectedAbilityId(out ushort abilityId)
        {
            return ushort.TryParse(_abilityIdTextBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out abilityId);
        }

        private async Task NavigateSelectedUnknownAbilityAsync()
        {
            OperationAbilityListRow selected = _unknownAbilityGrid.CurrentRow == null ? null : _unknownAbilityGrid.CurrentRow.DataBoundItem as OperationAbilityListRow;
            if (selected == null)
                return;

            await NavigateToAbilityAsync(selected.AbilityId, true);
        }

        private async Task NavigateSelectedRemainingWorkAbilityAsync()
        {
            RemainingWorkItemListRow selected = _remainingWorkItemGrid.CurrentRow == null ? null : _remainingWorkItemGrid.CurrentRow.DataBoundItem as RemainingWorkItemListRow;
            if (selected == null || !selected.ExampleAbilityId.HasValue)
                return;

            await NavigateToAbilityAsync(selected.ExampleAbilityId.Value, true);
        }

        private async Task NavigateSelectedConflictAbilityAsync()
        {
            ConflictListRow selected = _conflictGrid.CurrentRow == null ? null : _conflictGrid.CurrentRow.DataBoundItem as ConflictListRow;
            ushort abilityId;
            if (selected == null || !TryParseAbilityIdFromSubjectKey(selected.SubjectKey, out abilityId))
                return;

            await NavigateToAbilityAsync(abilityId, true);
        }

        private async Task NavigateToAbilityAsync(ushort abilityId, bool generateReport)
        {
            if (_mainTabs != null && _mainTabs.TabPages.Count > 0)
                _mainTabs.SelectedIndex = 0;

            _abilityIdTextBox.Text = abilityId.ToString(CultureInfo.InvariantCulture);
            SelectAbilityInCatalog(abilityId);
            AppendLog("Focused ability " + abilityId.ToString(CultureInfo.InvariantCulture) + " from triage view.");

            if (generateReport)
                await GenerateAbilityReportAsync();
        }

        private void SelectAbilityInCatalog(ushort abilityId)
        {
            if (_abilitySearchTextBox != null && !string.IsNullOrWhiteSpace(_abilitySearchTextBox.Text))
                _abilitySearchTextBox.Text = string.Empty;
            else
                ApplyAbilityFilter();

            if (_abilityGrid == null || _abilityGrid.Rows.Count == 0)
                return;

            foreach (DataGridViewRow row in _abilityGrid.Rows)
            {
                AbilityCatalogEntry entry = row.DataBoundItem as AbilityCatalogEntry;
                if (entry == null || entry.AbilityId != abilityId)
                    continue;

                if (row.Cells.Count > 0)
                    _abilityGrid.CurrentCell = row.Cells[0];
                return;
            }
        }

        private void PopulateAbilityPresentation(AbilityAnalysisResult report, string markdownPath)
        {
            _abilitySummaryTextBox.Text = BuildAbilitySummary(report, markdownPath);
            _abilityNarrativeTextBox.Text = _narrativeBuilder == null ? "Narrative builder is not initialized." : _narrativeBuilder.BuildNarrative(report);
            _definitionGrid.DataSource = _definitions == null ? new List<DefinitionEntry>() : _definitions.BuildAbilityDefinitions(report);
            UpdateDefinitionHint();
            BuildAbilityTree(report);
        }

        private void ClearAbilityPresentation(string summary)
        {
            _abilitySummaryTextBox.Text = summary;
            _abilityNarrativeTextBox.Text = "Generate an ability report to see what we think happens when the ability is used.";
            _definitionGrid.DataSource = null;
            UpdateDefinitionHint();
            _abilityTreeView.Nodes.Clear();
            _abilityTreeView.Nodes.Add(new TreeNode("Generate an ability report to populate this tree."));
        }

        private void BuildAbilityTree(AbilityAnalysisResult report)
        {
            _abilityTreeView.BeginUpdate();
            _abilityTreeView.Nodes.Clear();

            TreeNode root = new TreeNode(GetAbilityName(report) + " [" + report.AbilityId.ToString(CultureInfo.InvariantCulture) + "]");
            TreeNode summary = root.Nodes.Add("Summary");
            summary.Nodes.Add("Client rows: " + report.ClientAbilityRows.Count.ToString(CultureInfo.InvariantCulture));
            summary.Nodes.Add("BIN rows: " + report.BinaryAbilityRows.Count.ToString(CultureInfo.InvariantCulture));
            summary.Nodes.Add("Effect rows: " + report.ClientEffectRows.Count.ToString(CultureInfo.InvariantCulture));
            summary.Nodes.Add("Component rows: " + report.BinaryComponentRows.Count.ToString(CultureInfo.InvariantCulture));
            summary.Nodes.Add("Requirement links: " + (report.RequirementReferences == null ? 0 : report.RequirementReferences.Count).ToString(CultureInfo.InvariantCulture));
            summary.Nodes.Add("Requirement rows: " + report.BinaryRequirementRows.Count.ToString(CultureInfo.InvariantCulture));

            TreeNode warnings = root.Nodes.Add("Warnings");
            if (report.Warnings.Count == 0)
                warnings.Nodes.Add("No warnings.");
            else
                foreach (string warning in report.Warnings)
                    warnings.Nodes.Add(warning);

            TreeNode csv = root.Nodes.Add("abilities.csv");
            if (report.ClientAbilityRows.Count == 0)
                csv.Nodes.Add("No client CSV row found.");
            foreach (ClientAbilityRecord row in report.ClientAbilityRows)
            {
                TreeNode node = csv.Nodes.Add("Line " + row.LineNumber.ToString(CultureInfo.InvariantCulture));
                node.Nodes.Add("Name: " + NullToPlaceholder(row.Name));
                node.Nodes.Add("Description: " + NullToPlaceholder(row.Description));
                node.Nodes.Add("EffectId: " + row.EffectId.ToString(CultureInfo.InvariantCulture));
            }

            TreeNode bin = root.Nodes.Add("abilityexport.bin");
            if (report.BinaryAbilityRows.Count == 0)
                bin.Nodes.Add("No BIN row found.");
            foreach (BinaryAbilityRecord row in report.BinaryAbilityRows)
            {
                TreeNode node = bin.Nodes.Add("Record " + row.RecordIndex.ToString(CultureInfo.InvariantCulture) + " @ byte " + row.ByteOffset.ToString(CultureInfo.InvariantCulture));
                node.Nodes.Add("EffectId: " + row.EffectId.ToString(CultureInfo.InvariantCulture) + " -> " + EffectName(report, row.EffectId));
                node.Nodes.Add("CareerLine: " + row.CareerLine.ToString(CultureInfo.InvariantCulture) + " -> " + _definitions.DescribeCareerLine(row.CareerLine));
                node.Nodes.Add("TargetType: " + row.TargetType.ToString(CultureInfo.InvariantCulture) + " -> " + _definitions.DescribeTargetType(row.TargetType));
                node.Nodes.Add("AbilityType: " + row.AbilityType.ToString(CultureInfo.InvariantCulture) + " -> " + _definitions.DescribeAbilityType(row.AbilityType));
                node.Nodes.Add("AttackType: " + row.AttackType.ToString(CultureInfo.InvariantCulture) + " -> " + _definitions.DescribeAttackType(row.AttackType));
                node.Nodes.Add("TacticType: " + row.TacticType.ToString(CultureInfo.InvariantCulture) + " -> " + _definitions.DescribeTacticType(row.TacticType));
                node.Nodes.Add("CastTime: " + row.CastTime.ToString(CultureInfo.InvariantCulture) + " ms");
                node.Nodes.Add("Cooldown: " + row.Cooldown.ToString(CultureInfo.InvariantCulture) + " ms");
                node.Nodes.Add("Range: " + row.Range.ToString(CultureInfo.InvariantCulture) + " feet");
                node.Nodes.Add("AP Cost: " + row.ApCost.ToString(CultureInfo.InvariantCulture));
                TreeNode comp = node.Nodes.Add("Component Slots");
                for (int index = 0; index < row.ComponentIds.Count; ++index)
                {
                    ushort componentId = row.ComponentIds[index];
                    uint triggerValue = index < row.Triggers.Count ? row.Triggers[index] : 0;
                    if (componentId <= 0 && triggerValue == 0)
                        continue;
                    string text = "Slot " + index.ToString(CultureInfo.InvariantCulture) + ": Component " + componentId.ToString(CultureInfo.InvariantCulture);
                    BinaryComponentRecord componentRow = report.BinaryComponentRows.FirstOrDefault(x => x.ComponentId == componentId);
                    if (componentRow != null)
                        text += " -> " + _definitions.DescribeComponentOperation(componentRow.Operation);
                    if (triggerValue > 0)
                        text += " | Trigger " + triggerValue.ToString(CultureInfo.InvariantCulture) + " -> " + _definitions.DescribeTrigger(triggerValue);
                    comp.Nodes.Add(text);
                }
            }

            TreeNode effects = root.Nodes.Add("effects.csv");
            if (report.ClientEffectRows.Count == 0)
                effects.Nodes.Add("No effect rows found.");
            foreach (ClientEffectRecord row in report.ClientEffectRows.OrderBy(x => x.EffectId))
            {
                TreeNode node = effects.Nodes.Add("Effect " + row.EffectId.ToString(CultureInfo.InvariantCulture) + " - " + NullToPlaceholder(row.Name));
                node.Nodes.Add("BuildUp: " + TextOrNone(row.BuildUpId));
                node.Nodes.Add("Cast: " + TextOrNone(row.CastId));
                node.Nodes.Add("Activate: " + TextOrNone(row.ActivateId));
                node.Nodes.Add("Projectile: " + TextOrNone(row.ProjectileId));
                node.Nodes.Add("Impact: " + TextOrNone(row.ImpactId));
                node.Nodes.Add("AOE: " + TextOrNone(row.AoeId));
                node.Nodes.Add("Channel: " + TextOrNone(row.ChannelingId));
            }

            TreeNode components = root.Nodes.Add("Components");
            if (report.BinaryComponentRows.Count == 0)
                components.Nodes.Add("No component rows found.");
            foreach (BinaryComponentRecord row in report.BinaryComponentRows.OrderBy(x => x.ComponentId))
            {
                TreeNode node = components.Nodes.Add("Component " + row.ComponentId.ToString(CultureInfo.InvariantCulture) + " -> " + _definitions.DescribeComponentOperation(row.Operation));
                node.Nodes.Add("Duration: " + row.Duration.ToString(CultureInfo.InvariantCulture) + " ms");
                node.Nodes.Add("Interval: " + row.Interval.ToString(CultureInfo.InvariantCulture) + " ms");
                node.Nodes.Add("Radius: " + row.Radius.ToString(CultureInfo.InvariantCulture) + " feet");
                node.Nodes.Add("Description: " + NullToPlaceholder(row.Description));
                node.Nodes.Add("Values: " + string.Join(", ", row.Values.Select(x => x.ToString(CultureInfo.InvariantCulture))));
            }

            TreeNode requirementLinks = root.Nodes.Add("Requirement Links");
            if (report.RequirementReferences == null || report.RequirementReferences.Count == 0)
                requirementLinks.Nodes.Add("No inferred requirement links found.");
            else
            {
                foreach (RequirementReferenceRecord reference in report.RequirementReferences.OrderBy(x => x.SourceKind).ThenBy(x => x.SourceId).ThenBy(x => x.ExtSlotIndex).ThenBy(x => x.RequirementId))
                {
                    TreeNode node = requirementLinks.Nodes.Add(reference.SourceLabel + " -> Requirement " + reference.RequirementId.ToString(CultureInfo.InvariantCulture));
                    node.Nodes.Add("Field: " + reference.SourceField);
                    node.Nodes.Add("Confidence: " + reference.Confidence);
                    node.Nodes.Add("Location: " + reference.SourceLocation);
                    node.Nodes.Add("Notes: " + reference.Notes);
                }
            }

            TreeNode requirements = root.Nodes.Add("Requirements");
            if (report.BinaryRequirementRows == null || report.BinaryRequirementRows.Count == 0)
                requirements.Nodes.Add("No linked requirement rows found.");
            else
            {
                foreach (BinaryRequirementRecord row in report.BinaryRequirementRows.OrderBy(x => x.RequirementId).ThenBy(x => x.RecordIndex))
                {
                    TreeNode node = requirements.Nodes.Add("Requirement " + row.RequirementId.ToString(CultureInfo.InvariantCulture) + " @ byte " + row.ByteOffset.ToString(CultureInfo.InvariantCulture));
                    foreach (BinaryExtDataRecord ext in row.ExtData)
                    {
                        node.Nodes.Add("ExtData[" + ext.SlotIndex.ToString(CultureInfo.InvariantCulture) + "]: [" + ext.Val1.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val2.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val3.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val4.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val5.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val6.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val7.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val8.ToString(CultureInfo.InvariantCulture) + ", " + ext.Val9.ToString(CultureInfo.InvariantCulture) + "]");
                    }
                }
            }

            _abilityTreeView.Nodes.Add(root);
            root.Expand();
            foreach (TreeNode child in root.Nodes)
                child.Expand();
            _abilityTreeView.EndUpdate();
        }

        private void ApplyAbilityFilter()
        {
            string filter = (_abilitySearchTextBox.Text ?? string.Empty).Trim().ToLowerInvariant();
            bool hidePartial = _hidePartialAbilitiesCheckBox != null && _hidePartialAbilitiesCheckBox.Checked;
            
            var query = _abilityCatalog.AsEnumerable();
            
            if (hidePartial)
            {
                query = query.Where(x => x.HasClientCsv && x.HasClientBin);
            }
            
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(x => x.SearchText.Contains(filter));
            }
            
            List<AbilityCatalogEntry> filtered = query.OrderBy(x => x.AbilityId).ToList();
            
            _abilityGrid.DataSource = filtered;
            _abilityCatalogStatusLabel.Text = filtered.Count.ToString(CultureInfo.InvariantCulture) + " abilities shown out of " + _abilityCatalog.Count.ToString(CultureInfo.InvariantCulture) + ".";
            SelectFirstRow(_abilityGrid);
        }

        private AbilityCatalogEntry SelectedAbility()
        {
            return _abilityGrid.CurrentRow == null ? null : _abilityGrid.CurrentRow.DataBoundItem as AbilityCatalogEntry;
        }

        private void RefreshConflictRows()
        {
            if (_lastConflictReport == null)
            {
                _conflictGrid.DataSource = null;
                _conflictValueGrid.DataSource = null;
                _conflictClaimGrid.DataSource = null;
                _conflictInsightTextBox.Text = "Select a conflict to inspect its subject, decoded values, and source pattern.";
                return;
            }

            ConflictDomainRow selected = _conflictDomainGrid.CurrentRow == null ? null : _conflictDomainGrid.CurrentRow.DataBoundItem as ConflictDomainRow;
            IEnumerable<ConflictRecord> conflicts = GetVisibleConflicts();
            if (selected != null && !string.IsNullOrWhiteSpace(selected.Domain))
                conflicts = conflicts.Where(x => string.Equals(x.Domain, selected.Domain, StringComparison.OrdinalIgnoreCase));

            _conflictGrid.DataSource = conflicts.OrderByDescending(x => x.TriageScore).ThenBy(x => x.SubjectKey).ThenBy(x => x.FactName).Take(500).Select(x => new ConflictListRow
            {
                TriageBucket = x.TriageBucket,
                TriageScore = x.TriageScore,
                TriageCategory = x.TriageCategory,
                ResolutionHint = BuildConflictResolutionHint(x),
                SubjectKind = ResolveSubjectKind(x.SubjectKey),
                SubjectKey = x.SubjectKey,
                FactName = x.FactName,
                Domain = x.Domain,
                DistinctValues = string.Join(" | ", x.DistinctValues ?? new List<string>()),
                ClaimCount = x.Claims == null ? 0 : x.Claims.Count,
                Conflict = x
            }).ToList();
            SelectFirstRow(_conflictGrid);
            RefreshConflictClaims();
        }

        private void RefreshConflictClaims()
        {
            if (_lastConflictReport == null)
            {
                _conflictValueGrid.DataSource = null;
                _conflictClaimGrid.DataSource = null;
                _conflictInsightTextBox.Text = "Select a conflict to inspect its subject, decoded values, and source pattern.";
                return;
            }

            ConflictListRow selected = _conflictGrid.CurrentRow == null ? null : _conflictGrid.CurrentRow.DataBoundItem as ConflictListRow;
            if (selected == null || selected.Conflict == null)
            {
                _conflictValueGrid.DataSource = null;
                _conflictClaimGrid.DataSource = null;
                _conflictInsightTextBox.Text = "Select a conflict to inspect its subject, decoded values, and source pattern.";
                return;
            }

            _conflictInsightTextBox.Text = BuildConflictInsightSummary(selected.Conflict);
            _conflictValueGrid.DataSource = BuildConflictValueRows(selected.Conflict);
            _conflictClaimGrid.DataSource = (selected.Conflict.Claims ?? new List<ClaimRecord>()).Select(claim => new ConflictClaimListRow
            {
                NormalizedValue = NullToPlaceholder(claim.NormalizedValue),
                RawValue = NullToPlaceholder(claim.RawValue),
                SourceFamily = claim.SourceFamily,
                TableName = claim.TableName,
                FieldName = claim.FieldName,
                Confidence = claim.Confidence,
                SourcePath = claim.SourcePath,
                SourceLocation = claim.LineNumber > 0
                    ? "line " + claim.LineNumber.ToString(CultureInfo.InvariantCulture)
                    : (claim.ByteOffset > 0 ? "byte " + claim.ByteOffset.ToString(CultureInfo.InvariantCulture) : string.Empty),
                Notes = claim.Notes
            }).ToList();
        }

        private void RefreshConflictView()
        {
            if (_lastConflictReport == null)
            {
                _conflictSummaryTextBox.Text = "Generate the conflict ledger to browse contradiction domains.";
                _conflictDomainGrid.DataSource = null;
                _conflictGrid.DataSource = null;
                _conflictClaimGrid.DataSource = null;
                return;
            }

            _conflictSummaryTextBox.Text = BuildConflictSummary(_lastConflictReport, _lastConflictMarkdownPath);
            _conflictDomainGrid.DataSource = BuildDomainRows(_lastConflictReport);
            SelectFirstRow(_conflictDomainGrid);
            RefreshConflictRows();
        }

        private List<ConflictRecord> GetVisibleConflicts()
        {
            IEnumerable<ConflictRecord> conflicts = _lastConflictReport == null || _lastConflictReport.Conflicts == null
                ? Enumerable.Empty<ConflictRecord>()
                : _lastConflictReport.Conflicts;
            if (_hideBlankStringConflictCheckBox != null && _hideBlankStringConflictCheckBox.Checked)
                conflicts = conflicts.Where(conflict => !conflict.IsNoise);
            if (_hideMirrorEffectIdConflictCheckBox != null && _hideMirrorEffectIdConflictCheckBox.Checked)
                conflicts = conflicts.Where(conflict => !string.Equals(conflict.TriageCategory, "AbilityIdMirrorEffectId", StringComparison.OrdinalIgnoreCase));
            if (_highSignalConflictsOnlyCheckBox != null && _highSignalConflictsOnlyCheckBox.Checked)
                conflicts = conflicts.Where(conflict => string.Equals(conflict.TriageBucket, "Critical", StringComparison.OrdinalIgnoreCase) || string.Equals(conflict.TriageBucket, "High", StringComparison.OrdinalIgnoreCase));
            return conflicts.ToList();
        }

        private void RefreshDomainValues()
        {
            if (_lastDomainLedger == null)
            {
                _domainValuesGrid.DataSource = null;
                return;
            }

            DomainListRow selected = _domainsGrid.CurrentRow == null ? null : _domainsGrid.CurrentRow.DataBoundItem as DomainListRow;
            IdentityDomainRecord domain = selected == null
                ? _lastDomainLedger.Domains.FirstOrDefault()
                : _lastDomainLedger.Domains.FirstOrDefault(row => string.Equals(row.DomainKey, selected.DomainKey, StringComparison.OrdinalIgnoreCase));
            if (domain == null)
            {
                _domainValuesGrid.DataSource = null;
                return;
            }

            _domainValuesGrid.DataSource = (domain.Values ?? new List<IdentityDomainValueRecord>()).Select(row => new DomainValueListRow
            {
                RawValue = row.RawValue,
                Meaning = row.Meaning,
                Confidence = row.Confidence,
                Source = row.Source,
                SourcePath = row.SourcePath,
                SourceLocation = row.SourceLocation,
                Notes = row.Notes
            }).ToList();
        }

        private void RefreshRequirementDetails()
        {
            if (_lastRequirementLedger == null)
            {
                _requirementRowGrid.DataSource = null;
                _requirementFieldGrid.DataSource = null;
                _requirementReferenceGrid.DataSource = null;
                return;
            }

            RequirementListRow selected = _requirementsGrid.CurrentRow == null ? null : _requirementsGrid.CurrentRow.DataBoundItem as RequirementListRow;
            RequirementLedgerRecord requirement = selected == null
                ? _lastRequirementLedger.Requirements.FirstOrDefault()
                : _lastRequirementLedger.Requirements.FirstOrDefault(row => row.RequirementId == selected.RequirementId);
            if (requirement == null)
            {
                _requirementRowGrid.DataSource = null;
                _requirementFieldGrid.DataSource = null;
                _requirementReferenceGrid.DataSource = null;
                return;
            }

            _requirementRowGrid.DataSource = (requirement.Rows ?? new List<RequirementRowRecord>()).Select(row => new RequirementRowListRow
            {
                RecordIndex = row.RecordIndex,
                ExtDataText = row.ExtDataText,
                SourcePath = row.SourcePath,
                SourceLocation = row.SourceLocation
            }).ToList();
            _requirementFieldGrid.DataSource = (requirement.Fields ?? new List<RequirementFieldRecord>()).Select(row => new RequirementFieldListRow
            {
                FieldKey = row.FieldKey,
                NonZeroCount = row.NonZeroCount,
                DistinctValueCount = row.DistinctValueCount,
                SampleValuesText = row.SampleValuesText,
                SemanticSummary = row.SemanticSummary,
                Confidence = row.Confidence,
                Notes = row.Notes
            }).ToList();

            List<RequirementReferenceListRow> referenceRows = new List<RequirementReferenceListRow>();
            foreach (RequirementLinkEvidenceRecord reference in requirement.InboundReferences ?? new List<RequirementLinkEvidenceRecord>())
            {
                referenceRows.Add(new RequirementReferenceListRow
                {
                    Direction = "Inbound",
                    SourceKind = reference.SourceKind,
                    SourceId = reference.SourceId,
                    SourceLabel = reference.SourceLabel,
                    SourceField = reference.SourceField,
                    LinkedRequirementId = reference.LinkedRequirementId,
                    RelatedAbilitiesText = reference.RelatedAbilitiesText,
                    ContextTagsText = reference.ContextTagsText,
                    SourcePath = reference.SourcePath,
                    SourceLocation = reference.SourceLocation,
                    Notes = reference.Notes
                });
            }

            foreach (RequirementLinkEvidenceRecord reference in requirement.OutboundReferences ?? new List<RequirementLinkEvidenceRecord>())
            {
                referenceRows.Add(new RequirementReferenceListRow
                {
                    Direction = "Outbound",
                    SourceKind = reference.SourceKind,
                    SourceId = reference.SourceId,
                    SourceLabel = reference.SourceLabel,
                    SourceField = reference.SourceField,
                    LinkedRequirementId = reference.LinkedRequirementId,
                    RelatedAbilitiesText = reference.RelatedAbilitiesText,
                    ContextTagsText = reference.ContextTagsText,
                    SourcePath = reference.SourcePath,
                    SourceLocation = reference.SourceLocation,
                    Notes = reference.Notes
                });
            }

            _requirementReferenceGrid.DataSource = referenceRows
                .OrderBy(row => row.Direction)
                .ThenBy(row => row.SourceKind)
                .ThenBy(row => row.SourceId)
                .ThenBy(row => row.LinkedRequirementId)
                .ToList();
        }

        private void RefreshOperationDetails()
        {
            if (_lastOperationSchema == null)
            {
                _operationFieldGrid.DataSource = null;
                _operationAbilityGrid.DataSource = null;
                return;
            }

            OperationSchemaListRow selected = _operationGrid.CurrentRow == null ? null : _operationGrid.CurrentRow.DataBoundItem as OperationSchemaListRow;
            ComponentOperationSchemaRecord operation = selected == null
                ? _lastOperationSchema.Operations.FirstOrDefault()
                : _lastOperationSchema.Operations.FirstOrDefault(row => row.OperationId == selected.OperationId);
            if (operation == null)
            {
                _operationFieldGrid.DataSource = null;
                _operationAbilityGrid.DataSource = null;
                return;
            }

            _operationFieldGrid.DataSource = (operation.Fields ?? new List<ComponentOperationFieldRecord>()).Select(row => new OperationFieldListRow
            {
                FieldKey = row.FieldKey,
                TriageBucket = row.TriageBucket,
                TriageScore = row.TriageScore,
                NonZeroCount = row.NonZeroCount,
                DistinctValueCount = row.DistinctValueCount,
                SampleValuesText = row.SampleValuesText,
                TokenRenderingsText = row.TokenRenderingsText,
                SemanticSummary = row.SemanticSummary,
                Confidence = row.Confidence,
                ContextTagsText = row.ContextTagsText,
                Notes = string.IsNullOrWhiteSpace(row.TriageNotes) ? row.Notes : row.TriageNotes + " " + row.Notes
            }).ToList();
            _operationAbilityGrid.DataSource = (operation.SampleAbilities ?? new List<ComponentOperationAbilityRecord>()).Select(row => new OperationAbilityListRow
            {
                AbilityId = row.AbilityId,
                AbilityName = row.AbilityName,
                ComponentId = row.ComponentId,
                ComponentSlotIndex = row.ComponentSlotIndex,
                TriggerText = row.TriggerText,
                ContextTagsText = row.ContextTagsText,
                TextExcerpt = row.TextExcerpt,
                SourcePath = row.SourcePath,
                SourceLocation = row.SourceLocation
            }).ToList();
        }

        private void RefreshUnknownView()
        {
            if (_lastOperationSchema == null)
            {
                _unknownSummaryTextBox.Text = "Generate operation schemas to rank the remaining unknown or structural component fields by impact and priority.";
                _unknownFieldGrid.DataSource = null;
                _unknownValueGrid.DataSource = null;
                _unknownCompanionGrid.DataSource = null;
                _unknownAbilityGrid.DataSource = null;
                _unknownInsightTextBox.Text = "Select an unknown or structural field and raw value to inspect its trigger mix, context tags, and dominant companion fields.";
                return;
            }

            List<UnknownFieldListRow> allRows = (_lastOperationSchema.Operations ?? new List<ComponentOperationSchemaRecord>())
                .SelectMany(operation => (operation.Fields ?? new List<ComponentOperationFieldRecord>())
                    .Where(field => string.Equals(field.Confidence, SemanticConfidence.Unknown, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(field.Confidence, SemanticConfidence.Structural, StringComparison.OrdinalIgnoreCase))
                    .Select(field => new UnknownFieldListRow
                    {
                        TriageBucket = field.TriageBucket,
                        TriageScore = field.TriageScore,
                        TriageNotes = field.TriageNotes,
                        Confidence = field.Confidence,
                        PriorityText = operation.IsPriority ? "Yes" : "No",
                        OperationId = operation.OperationId,
                        OperationName = operation.OperationName,
                        FieldKey = field.FieldKey,
                        NonZeroCount = field.NonZeroCount,
                        DistinctValueCount = field.DistinctValueCount,
                        ContextTagsText = field.ContextTagsText,
                        SampleValuesText = field.SampleValuesText,
                        Notes = string.IsNullOrWhiteSpace(field.TriageNotes) ? field.Notes : field.TriageNotes + " " + field.Notes,
                        Operation = operation
                    }))
                .ToList();

            List<UnknownFieldListRow> rows = allRows;
            if (_hideMultiplierUnknownsCheckBox != null && _hideMultiplierUnknownsCheckBox.Checked)
                rows = rows.Where(row => !row.FieldKey.StartsWith("Multiplier[", StringComparison.OrdinalIgnoreCase)).ToList();

            rows = rows
                .OrderByDescending(row => row.TriageScore)
                .ThenByDescending(row => string.Equals(row.PriorityText, "Yes", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(row => row.NonZeroCount)
                .ThenByDescending(row => row.DistinctValueCount)
                .ThenBy(row => row.OperationId)
                .ThenBy(row => row.FieldKey)
                .ToList();

            _unknownSummaryTextBox.Text = BuildUnknownSummary(_lastOperationSchema, rows, allRows.Count - rows.Count);
            _unknownFieldGrid.DataSource = rows;
            SelectFirstRow(_unknownFieldGrid);
            RefreshUnknownDetails();
        }

        private void RefreshUnknownDetails()
        {
            if (_lastOperationSchema == null)
            {
                _unknownValueGrid.DataSource = null;
                _unknownCompanionGrid.DataSource = null;
                _unknownAbilityGrid.DataSource = null;
                _unknownInsightTextBox.Text = "Select an unknown or structural field and raw value to inspect its trigger mix, context tags, and dominant companion fields.";
                return;
            }

            UnknownFieldListRow selected = _unknownFieldGrid.CurrentRow == null ? null : _unknownFieldGrid.CurrentRow.DataBoundItem as UnknownFieldListRow;
            if (selected == null || selected.Operation == null)
            {
                _unknownValueGrid.DataSource = null;
                _unknownCompanionGrid.DataSource = null;
                _unknownAbilityGrid.DataSource = null;
                _unknownInsightTextBox.Text = "Select an unknown or structural field and raw value to inspect its trigger mix, context tags, and dominant companion fields.";
                return;
            }

            List<ComponentOperationFieldValueRecord> values = GetUnknownFieldValues(selected.OperationId, selected.FieldKey);
            _unknownValueGrid.DataSource = values.Select(row => new UnknownValueListRow
            {
                RawValue = row.RawValue,
                ObservationCount = row.ObservationCount,
                DistinctComponentCount = row.DistinctComponentCount,
                DistinctAbilityCount = row.DistinctAbilityCount,
                SampleComponentIdsText = row.SampleComponentIdsText,
                SampleAbilityIdsText = row.SampleAbilityIdsText,
                Value = row
            }).ToList();
            SelectFirstRow(_unknownValueGrid);
            RefreshUnknownValueAbilities();
        }

        private void RefreshUnknownValueAbilities()
        {
            UnknownValueListRow selected = _unknownValueGrid.CurrentRow == null ? null : _unknownValueGrid.CurrentRow.DataBoundItem as UnknownValueListRow;
            List<ComponentOperationAbilityRecord> abilities = selected == null || selected.Value == null
                ? new List<ComponentOperationAbilityRecord>()
                : selected.Value.SampleAbilities ?? new List<ComponentOperationAbilityRecord>();
            ComponentOperationFieldValueInsightRecord insight = selected == null || selected.Value == null
                ? null
                : GetUnknownValueInsight(selected.Value.RawValue);

            _unknownAbilityGrid.DataSource = abilities.Select(row => new OperationAbilityListRow
            {
                AbilityId = row.AbilityId,
                AbilityName = row.AbilityName,
                ComponentId = row.ComponentId,
                ComponentSlotIndex = row.ComponentSlotIndex,
                TriggerText = row.TriggerText,
                ContextTagsText = row.ContextTagsText,
                TextExcerpt = row.TextExcerpt,
                SourcePath = row.SourcePath,
                SourceLocation = row.SourceLocation
            }).ToList();
            _unknownCompanionGrid.DataSource = insight == null || insight.CompanionFields == null
                ? new List<UnknownCompanionListRow>()
                : insight.CompanionFields.Select(row => new UnknownCompanionListRow
                {
                    FieldKey = row.FieldKey,
                    DominantValue = row.DominantValue,
                    CoveragePercent = row.CoveragePercent,
                    MatchCount = row.MatchCount,
                    ObservationCount = row.ObservationCount,
                    DistinctValueCount = row.DistinctValueCount,
                    SampleValuesText = row.SampleValuesText
                }).ToList();
            _unknownInsightTextBox.Text = BuildUnknownValueInsightSummary(insight);
        }

        private List<ComponentOperationFieldValueRecord> GetUnknownFieldValues(uint operationId, string fieldKey)
        {
            if (_session == null || string.IsNullOrWhiteSpace(fieldKey))
                return new List<ComponentOperationFieldValueRecord>();

            string cacheKey = operationId.ToString(CultureInfo.InvariantCulture) + "|" + fieldKey;
            List<ComponentOperationFieldValueRecord> cached;
            if (_unknownValueCache.TryGetValue(cacheKey, out cached))
                return cached;

            List<ComponentOperationFieldValueRecord> rows = _session.BuildOperationFieldValueEvidence(operationId, fieldKey);
            _unknownValueCache[cacheKey] = rows;
            return rows;
        }

        private ComponentOperationFieldValueInsightRecord GetUnknownValueInsight(string rawValue)
        {
            if (_session == null || string.IsNullOrWhiteSpace(rawValue))
                return null;

            UnknownFieldListRow field = _unknownFieldGrid.CurrentRow == null ? null : _unknownFieldGrid.CurrentRow.DataBoundItem as UnknownFieldListRow;
            if (field == null)
                return null;

            string cacheKey = field.OperationId.ToString(CultureInfo.InvariantCulture)
                + "|"
                + field.FieldKey
                + "|"
                + rawValue;
            ComponentOperationFieldValueInsightRecord cached;
            if (_unknownInsightCache.TryGetValue(cacheKey, out cached))
                return cached;

            ComponentOperationFieldValueInsightRecord insight = _session.BuildOperationFieldValueInsight(field.OperationId, field.FieldKey, rawValue);
            _unknownInsightCache[cacheKey] = insight;
            return insight;
        }

        private string BuildDatasetSummary()
        {
            if (_session == null)
                return "Dataset is not loaded.";

            int loaded = _session.Dataset.TableStatuses.Count(x => x.Loaded);
            int failed = _session.Dataset.TableStatuses.Count(x => !x.Loaded);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Extracted root: " + _session.ExtractedRootPath);
            builder.AppendLine("Loaded sources: " + loaded.ToString(CultureInfo.InvariantCulture) + " / " + _session.Dataset.TableStatuses.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Failed sources: " + failed.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Client abilities: " + _session.Dataset.ClientAbilities.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BIN abilities: " + _session.Dataset.BinaryAbilities.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BIN components: " + _session.Dataset.BinaryComponents.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BIN requirements: " + _session.Dataset.BinaryRequirements.Count.ToString(CultureInfo.InvariantCulture));
            return builder.ToString().TrimEnd();
        }

        private string BuildAbilitySummary(AbilityAnalysisResult report, string markdownPath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(GetAbilityName(report) + " [" + report.AbilityId.ToString(CultureInfo.InvariantCulture) + "]");
            builder.AppendLine("Markdown: " + markdownPath);
            builder.AppendLine("Client rows: " + report.ClientAbilityRows.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BIN rows: " + report.BinaryAbilityRows.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Effect rows: " + report.ClientEffectRows.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Component rows: " + report.BinaryComponentRows.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Requirement links: " + (report.RequirementReferences == null ? 0 : report.RequirementReferences.Count).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Requirement rows: " + report.BinaryRequirementRows.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Conflicts: " + report.Graph.GetConflicts().Count.ToString(CultureInfo.InvariantCulture));
            if (report.Warnings.Count > 0)
            {
                builder.AppendLine("Warnings:");
                foreach (string warning in report.Warnings)
                    builder.AppendLine("- " + warning);
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildConflictSummary(ConflictReportDocument report, string markdownPath)
        {
            List<ConflictRecord> visibleConflicts = GetVisibleConflicts();
            int domainCount = visibleConflicts.Select(x => x.Domain ?? string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            int blankNoiseCount = report.Conflicts.Count(x => x.IsNoise);
            int placeholderStringCount = report.Conflicts.Count(x => string.Equals(x.TriageCategory, "PlaceholderStringMismatch", StringComparison.OrdinalIgnoreCase));
            int internalAbilityNameCount = report.Conflicts.Count(x => string.Equals(x.TriageCategory, "InternalAbilityNameMismatch", StringComparison.OrdinalIgnoreCase));
            int internalOnlyAbilityNameCount = report.Conflicts.Count(x => string.Equals(x.TriageCategory, "InternalOnlyAbilityNameMismatch", StringComparison.OrdinalIgnoreCase));
            int mirrorEffectIdCount = report.Conflicts.Count(x => string.Equals(x.TriageCategory, "AbilityIdMirrorEffectId", StringComparison.OrdinalIgnoreCase));
            int mountOverlayCount = report.Conflicts.Count(x => string.Equals(x.TriageCategory, "MountOverlayEffectId", StringComparison.OrdinalIgnoreCase));
            int zeroVsEffectCount = report.Conflicts.Count(x => string.Equals(x.TriageCategory, "ZeroVsEffectIdGap", StringComparison.OrdinalIgnoreCase));
            int resolvedEffectSuggestions = report.Conflicts.Count(x => string.Equals(x.Domain, "EffectId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.CanonicalValue));
            int resolvedAbilityNameSuggestions = report.Conflicts.Count(x => string.Equals(x.Domain, "AbilityName", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.CanonicalValue));
            int suppressed = report.Conflicts.Count - visibleConflicts.Count;
            int highSignal = visibleConflicts.Count(x => string.Equals(x.TriageBucket, "Critical", StringComparison.OrdinalIgnoreCase) || string.Equals(x.TriageBucket, "High", StringComparison.OrdinalIgnoreCase));
            return "Markdown: " + markdownPath + Environment.NewLine
                + "Claims: " + report.Claims.Count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Visible Conflicts: " + visibleConflicts.Count.ToString(CultureInfo.InvariantCulture) + " / " + report.Conflicts.Count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Domains: " + domainCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "High-Signal Conflicts: " + highSignal.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Blank/String Noise Cases: " + blankNoiseCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Placeholder/Test String Cases: " + placeholderStringCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Internal-vs-Player Ability Name Cases: " + internalAbilityNameCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Internal-Only Ability Name Cases: " + internalOnlyAbilityNameCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "AbilityId-Mirror EffectId Cases: " + mirrorEffectIdCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Mount Overlay EffectId Cases: " + mountOverlayCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Zero-vs-EffectId Gaps: " + zeroVsEffectCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "EffectId Rows With Canonical Suggestion: " + resolvedEffectSuggestions.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "AbilityName Rows With Canonical Suggestion: " + resolvedAbilityNameSuggestions.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Currently Hidden By Filters: " + suppressed.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "The detail grid is filtered by the selected domain, capped at 500 rows, and now shows claim-level evidence for the selected conflict.";
        }

        private string BuildTokenDictionarySummary(TokenDictionaryDocument report, string markdownPath)
        {
            int count = report == null || report.Definitions == null ? 0 : report.Definitions.Count;
            return "Markdown: " + markdownPath + Environment.NewLine
                + "Token grammar: " + (report == null ? string.Empty : report.TokenGrammar) + Environment.NewLine
                + "Definitions: " + count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "The dictionary explains COM tokens in plain English using extracted client text evidence first, with Londo reserved for last-resort overrides.";
        }

        private string BuildDomainSummary(DomainLedgerDocument report, string markdownPath)
        {
            List<IdentityDomainRecord> domains = report == null || report.Domains == null ? new List<IdentityDomainRecord>() : report.Domains;
            return "Markdown: " + markdownPath + Environment.NewLine
                + "Domains: " + domains.Count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Confirmed: " + domains.Count(row => string.Equals(row.Confidence, SemanticConfidence.Confirmed, StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Inferred: " + domains.Count(row => string.Equals(row.Confidence, SemanticConfidence.Inferred, StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Londo: " + domains.Count(row => string.Equals(row.Confidence, SemanticConfidence.Londo, StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "This ledger keeps `Race.EntryId`, `CareerLine.EntryId`, and repeated `CareerName.EntryId` rows separate so numeric domains do not get renamed into facts the extracted client files never proved.";
        }

        private string BuildRequirementSummary(RequirementLedgerDocument report, string markdownPath)
        {
            List<RequirementLedgerRecord> rows = report == null || report.Requirements == null ? new List<RequirementLedgerRecord>() : report.Requirements;
            return "Markdown: " + markdownPath + Environment.NewLine
                + "Requirements: " + rows.Count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Direct ability usage: " + rows.Count(row => row.DirectAbilityCount > 0).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Component-only usage: " + rows.Count(row => row.DirectAbilityCount <= 0 && row.DirectComponentCount > 0).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Nested child links: " + rows.Count(row => row.ChildRequirementCount > 0).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "This ledger keeps requirement decoding evidence-based by showing exactly which abilities, components, and other requirement rows point at each RequirementId.";
        }

        private string BuildCoverageSummary(CoverageReportDocument report, string markdownPath)
        {
            List<CoverageAbilityRecord> rows = report == null || report.Abilities == null ? new List<CoverageAbilityRecord>() : report.Abilities;
            return "Markdown: " + markdownPath + Environment.NewLine
                + "Abilities: " + rows.Count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "MappedWithRequirements: " + rows.Count(row => string.Equals(row.CoverageStatus, "MappedWithRequirements", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Mapped: " + rows.Count(row => string.Equals(row.CoverageStatus, "Mapped", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "PlayableSurface: " + rows.Count(row => string.Equals(row.CoverageStatus, "PlayableSurface", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "PartialOrWorse: " + rows.Count(row => !string.Equals(row.CoverageStatus, "MappedWithRequirements", StringComparison.OrdinalIgnoreCase) && !string.Equals(row.CoverageStatus, "Mapped", StringComparison.OrdinalIgnoreCase) && !string.Equals(row.CoverageStatus, "PlayableSurface", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "This ledger shows which extracted-client abilities have the minimum pieces needed for a strong analysis and which ones are still missing strings, effects, components, or requirement evidence.";
        }

        private string BuildRemainingWorkSummary(RemainingWorkDocument report, string markdownPath)
        {
            RemainingWorkSummaryRecord summary = report == null ? null : report.Summary;
            List<RemainingWorkAreaRecord> areas = report == null || report.Areas == null ? new List<RemainingWorkAreaRecord>() : report.Areas;
            RemainingWorkAreaRecord topArea = areas.OrderByDescending(row => row.PeakScore).ThenByDescending(row => row.ItemCount).FirstOrDefault();
            string nextMarkdownPath = string.IsNullOrWhiteSpace(markdownPath) ? string.Empty : Path.Combine(Path.GetDirectoryName(markdownPath), "remaining-work-next.md");
            string operationPacketPath = string.IsNullOrWhiteSpace(markdownPath) ? string.Empty : Path.Combine(Path.GetDirectoryName(markdownPath), "remaining-work-operation-fields.md");
            string literalCrosswalkPath = string.IsNullOrWhiteSpace(markdownPath) ? string.Empty : Path.Combine(Path.GetDirectoryName(markdownPath), "remaining-work-control-literals.md");
            return "Markdown: " + markdownPath + Environment.NewLine
                + "Next batch: " + nextMarkdownPath + Environment.NewLine
                + "Field packets: " + operationPacketPath + Environment.NewLine
                + "Literal crosswalk: " + literalCrosswalkPath + Environment.NewLine
                + "Areas: " + (summary == null ? 0 : summary.AreaCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Items: " + (summary == null ? 0 : summary.ItemCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Critical: " + (summary == null ? 0 : summary.CriticalCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "High: " + (summary == null ? 0 : summary.HighCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Coverage gaps: " + (summary == null ? 0 : summary.CoverageGapAbilityCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "High-signal conflicts: " + (summary == null ? 0 : summary.HighSignalConflictCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Unknown fields: " + (summary == null ? 0 : summary.UnknownFieldCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Structural fields: " + (summary == null ? 0 : summary.StructuralFieldCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Requirement rows with unresolved fields: " + (summary == null ? 0 : summary.RequirementGapCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Token gaps: " + (summary == null ? 0 : summary.TokenGapCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Identity-domain risks: " + (summary == null ? 0 : summary.DomainIssueCount).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + (topArea == null
                    ? "Generate the report to rank the backlog."
                    : "Top pressure area: " + topArea.Title + " [" + topArea.PeakBucket + " " + topArea.PeakScore.ToString(CultureInfo.InvariantCulture) + "] with " + topArea.ItemCount.ToString(CultureInfo.InvariantCulture) + " items.");
        }

        private string BuildOperationSummary(OperationSchemaDocument report, string markdownPath)
        {
            int operationCount = report == null || report.Operations == null ? 0 : report.Operations.Count;
            int fieldCount = report == null || report.Operations == null ? 0 : report.Operations.Sum(row => row.Fields == null ? 0 : row.Fields.Count);
            int priorityCount = report == null || report.Operations == null ? 0 : report.Operations.Count(row => row.IsPriority);
            return "Markdown: " + markdownPath + Environment.NewLine
                + "Operations: " + operationCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Field schemas: " + fieldCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Priority operations: " + priorityCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "This ledger groups component rows by operation, shows recurring non-zero fields, and explains them from extracted client evidence before any Londo override.";
        }

        private string BuildUnknownSummary(OperationSchemaDocument report, List<UnknownFieldListRow> rows, int suppressedCount)
        {
            int priorityRows = rows.Count(row => string.Equals(row.PriorityText, "Yes", StringComparison.OrdinalIgnoreCase));
            int priorityOperations = rows.Where(row => string.Equals(row.PriorityText, "Yes", StringComparison.OrdinalIgnoreCase)).Select(row => row.OperationId).Distinct().Count();
            int structuralRows = rows.Count(row => string.Equals(row.Confidence, SemanticConfidence.Structural, StringComparison.OrdinalIgnoreCase));
            UnknownFieldListRow top = rows.FirstOrDefault();
            return "Markdown: " + _lastOperationSchemaMarkdownPath + Environment.NewLine
                + "Unknown/Structural fields: " + rows.Count.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Structural role hints: " + structuralRows.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Priority hotspots: " + priorityRows.ToString(CultureInfo.InvariantCulture) + " fields across " + priorityOperations.ToString(CultureInfo.InvariantCulture) + " priority operations" + Environment.NewLine
                + "Suppressed multiplier noise: " + suppressedCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Operations with unknowns: " + (report == null || report.Operations == null ? 0 : report.Operations.Count(operation => (operation.Fields ?? new List<ComponentOperationFieldRecord>()).Any(field => string.Equals(field.Confidence, SemanticConfidence.Unknown, StringComparison.OrdinalIgnoreCase) || string.Equals(field.Confidence, SemanticConfidence.Structural, StringComparison.OrdinalIgnoreCase)))).ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + (top == null
                    ? "Generate operation schemas to begin triage."
                    : "Top hotspot: " + top.OperationName + " :: " + top.FieldKey + " [" + top.Confidence + " | " + top.TriageBucket + " " + top.TriageScore.ToString(CultureInfo.InvariantCulture) + "] (" + top.NonZeroCount.ToString(CultureInfo.InvariantCulture) + " non-zero rows).");
        }

        private static string BuildUnknownValueInsightSummary(ComponentOperationFieldValueInsightRecord insight)
        {
            if (insight == null)
                return "Select an unknown field and raw value to inspect its trigger mix, context tags, and dominant companion fields.";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Selected value: " + NullToPlaceholder(insight.RawValue));
            builder.AppendLine("Components: " + insight.DistinctComponentCount.ToString(CultureInfo.InvariantCulture)
                + " | Abilities: " + insight.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture)
                + " | Observations: " + insight.ObservationCount.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Triggers: " + NullToPlaceholder(insight.TriggerSummaryText));
            builder.AppendLine("Context tags: " + NullToPlaceholder(insight.ContextTagSummaryText));
            builder.Append("Strong companions: " + NullToPlaceholder(insight.CompanionSummaryText));
            return builder.ToString();
        }

        private string BuildConflictInsightSummary(ConflictRecord conflict)
        {
            if (conflict == null)
                return "Select a conflict to inspect its subject, decoded values, and source pattern.";

            List<string> sources = (conflict.Claims ?? new List<ClaimRecord>())
                .Select(claim => claim.SourceFamily)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Subject: " + DescribeConflictSubject(conflict.SubjectKey));
            builder.AppendLine("Fact: " + NullToPlaceholder(conflict.FactName) + " | Domain: " + NullToPlaceholder(conflict.Domain));
            builder.AppendLine("Triage: " + NullToPlaceholder(conflict.TriageBucket) + " " + conflict.TriageScore.ToString(CultureInfo.InvariantCulture) + " | " + NullToPlaceholder(conflict.TriageCategory));
            builder.AppendLine("Sources: " + (sources.Count == 0 ? "(none)" : string.Join(", ", sources)));
            builder.AppendLine("Distinct values: " + (conflict.DistinctValues == null ? 0 : conflict.DistinctValues.Count).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Resolution: " + BuildConflictResolutionHint(conflict));
            if (!string.IsNullOrWhiteSpace(conflict.ResolutionNotes))
                builder.AppendLine("Resolution notes: " + conflict.ResolutionNotes);
            builder.Append("Notes: " + NullToPlaceholder(conflict.TriageNotes));
            return builder.ToString();
        }

        private List<ConflictValueListRow> BuildConflictValueRows(ConflictRecord conflict)
        {
            if (conflict == null || conflict.Claims == null)
                return new List<ConflictValueListRow>();

            return conflict.Claims
                .GroupBy(claim => (claim.NormalizedValue ?? string.Empty) + "|" + (claim.RawValue ?? string.Empty), StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    List<ClaimRecord> claims = group.ToList();
                    ClaimRecord representative = claims[0];
                    string valueText = string.IsNullOrWhiteSpace(representative.RawValue) ? representative.NormalizedValue : representative.RawValue;
                    return new ConflictValueListRow
                    {
                        ValueText = NullToPlaceholder(valueText),
                        Meaning = DescribeConflictValue(conflict, representative),
                        SourcesText = string.Join(", ", claims.Select(claim => claim.SourceFamily).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase)),
                        ClaimCount = claims.Count,
                        Notes = BuildConflictValueNotes(conflict, representative, claims)
                    };
                })
                .OrderByDescending(row => row.ClaimCount)
                .ThenBy(row => row.ValueText)
                .ToList();
        }

        private List<ConflictDomainRow> BuildDomainRows(ConflictReportDocument report)
        {
            return GetVisibleConflicts().GroupBy(x => x.Domain ?? string.Empty, StringComparer.OrdinalIgnoreCase).Select(group => new ConflictDomainRow
            {
                Domain = group.Key,
                PeakTriageScore = group.Max(x => x.TriageScore),
                HighSignalCount = group.Count(x => string.Equals(x.TriageBucket, "Critical", StringComparison.OrdinalIgnoreCase) || string.Equals(x.TriageBucket, "High", StringComparison.OrdinalIgnoreCase)),
                ConflictCount = group.Count(),
                SubjectCount = group.Select(x => x.SubjectKey).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                FactCount = group.Select(x => x.FactName).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            }).OrderByDescending(x => x.HighSignalCount).ThenByDescending(x => x.PeakTriageScore).ThenByDescending(x => x.ConflictCount).ThenBy(x => x.Domain).ToList();
        }

        private void RefreshRemainingWorkItems()
        {
            if (_lastRemainingWorkReport == null)
            {
                _remainingWorkItemGrid.DataSource = null;
                _remainingWorkDetailTextBox.Text = "Generate the remaining-work report to inspect the ranked backlog.";
                return;
            }

            RemainingWorkAreaListRow selectedArea = _remainingWorkAreaGrid.CurrentRow == null ? null : _remainingWorkAreaGrid.CurrentRow.DataBoundItem as RemainingWorkAreaListRow;
            string areaKey = _remainingWorkAllAreasCheckBox != null && _remainingWorkAllAreasCheckBox.Checked
                ? null
                : selectedArea == null ? null : selectedArea.AreaKey;
            string minimumPriorityBucket = _remainingWorkPriorityFilterComboBox == null || _remainingWorkPriorityFilterComboBox.SelectedItem == null
                ? null
                : _remainingWorkPriorityFilterComboBox.SelectedItem.ToString();
            string searchText = _remainingWorkSearchTextBox == null ? null : _remainingWorkSearchTextBox.Text;
            int topCount = _remainingWorkTopCountUpDown == null ? 0 : Decimal.ToInt32(_remainingWorkTopCountUpDown.Value);
            List<RemainingWorkItemRecord> items = RemainingWorkCatalog.FilterItems(_lastRemainingWorkReport, areaKey, minimumPriorityBucket, searchText, topCount);

            _remainingWorkItemGrid.DataSource = items.Select(row => new RemainingWorkItemListRow
            {
                Rank = row.Rank,
                GlobalRank = row.GlobalRank,
                PriorityBucket = row.PriorityBucket,
                PriorityScore = row.PriorityScore,
                SubjectKind = row.SubjectKind,
                SubjectKey = row.SubjectKey,
                Title = row.Title,
                Summary = row.Summary,
                Evidence = row.Evidence,
                RecommendedAction = row.RecommendedAction,
                ExampleAbilityId = row.ExampleAbilityId,
                ExampleAbilityIdText = row.ExampleAbilityId.HasValue ? row.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                ReferencePath = row.ReferencePath,
                ReferenceLocation = row.ReferenceLocation
            }).ToList();
            if (_remainingWorkItemGrid.Rows.Count > 0)
                SelectFirstRow(_remainingWorkItemGrid);
            RefreshRemainingWorkDetail();
        }

        private void RefreshRemainingWorkDetail()
        {
            RemainingWorkItemListRow selected = _remainingWorkItemGrid.CurrentRow == null ? null : _remainingWorkItemGrid.CurrentRow.DataBoundItem as RemainingWorkItemListRow;
            if (selected == null)
            {
                _remainingWorkDetailTextBox.Text = _remainingWorkItemGrid.Rows.Count <= 0
                    ? "No remaining-work items match the current filters."
                    : "Select a remaining-work item to inspect its summary, evidence, and suggested next action.";
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Title: " + NullToPlaceholder(selected.Title));
            builder.AppendLine("Priority: " + NullToPlaceholder(selected.PriorityBucket) + " " + selected.PriorityScore.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Ranks: area " + selected.Rank.ToString(CultureInfo.InvariantCulture) + ", global " + selected.GlobalRank.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Subject: " + NullToPlaceholder(selected.SubjectKind) + " :: " + NullToPlaceholder(selected.SubjectKey));
            builder.AppendLine("Example ability: " + (selected.ExampleAbilityId.HasValue ? selected.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : "(none)"));
            builder.AppendLine();
            builder.AppendLine("Summary:");
            builder.AppendLine(NullToPlaceholder(selected.Summary));
            builder.AppendLine();
            builder.AppendLine("Evidence:");
            builder.AppendLine(NullToPlaceholder(selected.Evidence));
            builder.AppendLine();
            builder.AppendLine("Recommended next action:");
            builder.AppendLine(NullToPlaceholder(selected.RecommendedAction));

            if (!string.IsNullOrWhiteSpace(selected.ReferencePath) || !string.IsNullOrWhiteSpace(selected.ReferenceLocation))
            {
                builder.AppendLine();
                builder.AppendLine("Reference:");
                builder.AppendLine(NullToPlaceholder(selected.ReferencePath) + " " + NullToPlaceholder(selected.ReferenceLocation));
            }

            _remainingWorkDetailTextBox.Text = builder.ToString().TrimEnd();
        }

        private string ResolveOutputRoot()
        {
            return _session == null ? Path.GetFullPath(_outputPathTextBox.Text.Trim()) : _session.ResolveOutputRoot(_outputPathTextBox.Text.Trim());
        }

        private void SetBusy(bool isBusy, string statusText)
        {
            _isBusy = isBusy;
            UseWaitCursor = isBusy;
            Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
            _reloadButton.Enabled = !isBusy;
            _generateAllButton.Enabled = !isBusy && _session != null;
            _cleanWorkspaceButton.Enabled = !isBusy;
            _generateAbilityButton.Enabled = !isBusy && _session != null;
            _generateTokenDictionaryButton.Enabled = !isBusy && _session != null;
            _generateDomainsButton.Enabled = !isBusy && _session != null;
            _generateRequirementsButton.Enabled = !isBusy && _session != null;
            _generateCoverageButton.Enabled = !isBusy && _session != null;
            _generateRemainingWorkButton.Enabled = !isBusy && _session != null;
            _generateOperationSchemasButton.Enabled = !isBusy && _session != null;
            _generateUnknownsButton.Enabled = !isBusy && _session != null;
            _generateConflictsButton.Enabled = !isBusy && _session != null;
            _statusLabel.Text = statusText;
        }

        private void ChooseFolder(TextBox target, string description)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.SelectedPath = Directory.Exists(target.Text) ? target.Text : Environment.CurrentDirectory;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    target.Text = dialog.SelectedPath;
            }
        }

        private void OpenPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show(this, "No output path is available yet.", "ClientDataMatrix", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                MessageBox.Show(this, "The target path does not exist:" + Environment.NewLine + path, "ClientDataMatrix", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Process.Start(path);
        }

        private void CleanWorkspace()
        {
            if (_isBusy)
                return;

            try
            {
                WorkspaceCleanupReport report = WorkspaceCleanupService.CleanWorkspace(Environment.CurrentDirectory);
                AppendLog(report.BuildSummary());
                foreach (string removedPath in report.RemovedPaths)
                    AppendLog("Removed temp artifact: " + removedPath);
                foreach (string failedPath in report.FailedPaths)
                    AppendLog("Failed to remove temp artifact: " + failedPath);

                _statusLabel.Text = report.HasFailures ? "Workspace cleanup completed with errors." : "Workspace cleanup completed.";
                MessageBox.Show(
                    this,
                    report.BuildSummary(),
                    "ClientDataMatrix",
                    MessageBoxButtons.OK,
                    report.HasFailures ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("Workspace cleanup failed: " + ex.Message);
                _statusLabel.Text = "Workspace cleanup failed.";
                MessageBox.Show(this, ex.ToString(), "ClientDataMatrix Cleanup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AppendLog(string message)
        {
            _logTextBox.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "] " + message + Environment.NewLine);
        }

        private Control CreateDefinitionPanel()
        {
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _definitionHintLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(8),
                Text = "Generate an ability report, then double-click a definition row to inspect every known raw value, meaning, and source provenance for that field."
            };

            _definitionGrid = CreateReadOnlyGrid();
            _definitionGrid.AutoGenerateColumns = false;
            _definitionGrid.Columns.Add(CreateTextColumn("Field", "FieldPath", 220));
            _definitionGrid.Columns.Add(CreateTextColumn("Raw", "RawValue", 80));
            _definitionGrid.Columns.Add(CreateTextColumn("Meaning", "Meaning", 320));
            _definitionGrid.Columns.Add(CreateTextColumn("Confidence", "Confidence", 90));
            _definitionGrid.Columns.Add(CreateTextColumn("Source", "DefinitionSource", 180));
            _definitionGrid.Columns.Add(CreateTextColumn("Value From", "ValueSource", 140));
            _definitionGrid.Columns.Add(CreateTextColumn("Location", "ValueSourceLocation", 90));
            _definitionGrid.Columns.Add(CreateTextColumn("Notes", "Notes", 220));
            _definitionGrid.CellDoubleClick += (sender, args) => { if (args.RowIndex >= 0) ShowDefinitionExplorer(); };

            layout.Controls.Add(_definitionHintLabel, 0, 0);
            layout.Controls.Add(_definitionGrid, 0, 1);
            return layout;
        }

        private void ShowDefinitionExplorer()
        {
            if (_definitions == null)
                return;

            DefinitionEntry entry = _definitionGrid.CurrentRow == null ? null : _definitionGrid.CurrentRow.DataBoundItem as DefinitionEntry;
            if (entry == null)
                return;

            DefinitionExplorerModel model = _definitions.BuildExplorer(entry);
            using (DefinitionExplorerForm dialog = new DefinitionExplorerForm(model))
                dialog.ShowDialog(this);

            AppendLog("Opened definition explorer for " + entry.FieldPath + ".");
        }

        private void UpdateDefinitionHint()
        {
            List<DefinitionEntry> rows = _definitionGrid == null ? null : _definitionGrid.DataSource as List<DefinitionEntry>;
            int count = rows == null ? 0 : rows.Count;
            if (_definitionHintLabel != null)
                _definitionHintLabel.Text = count <= 0
                    ? "Generate an ability report, then double-click a definition row to inspect every known raw value, meaning, and source provenance for that field."
                    : count.ToString(CultureInfo.InvariantCulture) + " decoded rows loaded. Double-click a row to open its full client-domain ledger.";
        }

        private string DescribeConflictSubject(string subjectKey)
        {
            ushort abilityId;
            if (TryParseAbilityIdFromSubjectKey(subjectKey, out abilityId))
            {
                AbilityCatalogEntry match = _abilityCatalog.FirstOrDefault(row => row.AbilityId == abilityId);
                return match == null ? subjectKey : "Ability " + abilityId.ToString(CultureInfo.InvariantCulture) + " - " + NullToPlaceholder(match.Name);
            }

            int effectId;
            if (TryParseEffectIdFromSubjectKey(subjectKey, out effectId))
                return "Effect " + effectId.ToString(CultureInfo.InvariantCulture) + " - " + LookupEffectMeaning(effectId);

            return NullToPlaceholder(subjectKey);
        }

        private string DescribeConflictValue(ConflictRecord conflict, ClaimRecord claim)
        {
            if (conflict == null || claim == null)
                return string.Empty;

            string rawValue = string.IsNullOrWhiteSpace(claim.RawValue) ? claim.NormalizedValue : claim.RawValue;
            if (string.IsNullOrWhiteSpace(rawValue))
                return "(blank)";

            if (string.Equals(conflict.Domain, "EffectId", StringComparison.OrdinalIgnoreCase))
            {
                int effectId;
                if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out effectId))
                {
                    List<string> parts = new List<string> { LookupEffectMeaning(effectId) };
                    int subjectAbilityId;
                    if (TryParseAbilityId(conflict.SubjectKey, out subjectAbilityId) && effectId == subjectAbilityId)
                        parts.Add("matches AbilityId");
                    return string.Join(" | ", parts.Where(value => !string.IsNullOrWhiteSpace(value)));
                }
            }

            long numericValue;
            if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
            {
                if (string.Equals(conflict.Domain, "Milliseconds", StringComparison.OrdinalIgnoreCase))
                    return numericValue.ToString(CultureInfo.InvariantCulture) + " ms";
                if (string.Equals(conflict.Domain, "Feet", StringComparison.OrdinalIgnoreCase))
                    return numericValue.ToString(CultureInfo.InvariantCulture) + " feet";
                if (string.Equals(conflict.Domain, "ActionPoints", StringComparison.OrdinalIgnoreCase))
                    return numericValue.ToString(CultureInfo.InvariantCulture) + " action points";
            }

            return NullToPlaceholder(claim.NormalizedValue);
        }

        private string BuildConflictValueNotes(ConflictRecord conflict, ClaimRecord representative, List<ClaimRecord> claims)
        {
            List<string> notes = new List<string>();
            if (representative != null && !string.IsNullOrWhiteSpace(representative.FieldName))
                notes.Add("field " + representative.FieldName);

            if (string.Equals(conflict == null ? string.Empty : conflict.TriageCategory, "AbilityIdMirrorEffectId", StringComparison.OrdinalIgnoreCase))
            {
                int subjectAbilityId;
                int value;
                string rawValue = representative == null || string.IsNullOrWhiteSpace(representative.RawValue) ? string.Empty : representative.RawValue;
                if (TryParseAbilityId(conflict == null ? null : conflict.SubjectKey, out subjectAbilityId)
                    && int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                    && value == subjectAbilityId)
                    notes.Add("mirrors subject AbilityId");
                else
                    notes.Add("non-mirror target value");
            }

            string sourceFiles = string.Join(", ", claims.Select(claim => claim.TableName).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(sourceFiles))
                notes.Add(sourceFiles);

            return string.Join("; ", notes);
        }

        private static string BuildConflictResolutionHint(ConflictRecord conflict)
        {
            if (conflict == null)
                return "(none)";

            List<string> parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(conflict.RecommendedSourceFamily))
                parts.Add(conflict.RecommendedSourceFamily);
            if (!string.IsNullOrWhiteSpace(conflict.CanonicalValue))
                parts.Add(conflict.CanonicalValue);
            if (!string.IsNullOrWhiteSpace(conflict.CanonicalMeaning)
                && !string.Equals(conflict.CanonicalMeaning, conflict.CanonicalValue, StringComparison.OrdinalIgnoreCase))
                parts.Add(conflict.CanonicalMeaning);
            if (parts.Count > 0)
                return string.Join(" -> ", parts);
            if (!string.IsNullOrWhiteSpace(conflict.ResolutionRule))
                return conflict.ResolutionRule;
            return "(none)";
        }

        private string LookupEffectMeaning(int effectId)
        {
            if (_definitions != null)
                return _definitions.DescribeEffectId(effectId);

            if (_session != null)
            {
                ClientEffectRecord row = _session.Dataset.ClientEffects.FirstOrDefault(effect => effect.EffectId == (uint)effectId && !string.IsNullOrWhiteSpace(effect.Name));
                if (row != null)
                    return row.Name;
            }

            return "Unknown effect " + effectId.ToString(CultureInfo.InvariantCulture);
        }

        private static TextBox CreateReadOnlyTextBox() { return new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, ReadOnly = true, WordWrap = true }; }
        private static DataGridView CreateReadOnlyGrid() { return new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, ReadOnly = true, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells, ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false }; }
        private static DataGridViewTextBoxColumn CreateTextColumn(string headerText, string propertyName, int width) { return new DataGridViewTextBoxColumn { HeaderText = headerText, DataPropertyName = propertyName, Width = width, MinimumWidth = Math.Min(width, 60), SortMode = DataGridViewColumnSortMode.Automatic }; }
        private static GroupBox WrapInGroup(string title, Control content) { GroupBox group = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(10) }; group.Controls.Add(content); return group; }
        private static void SelectFirstRow(DataGridView grid) { if (grid != null && grid.Rows.Count > 0 && grid.CurrentCell == null) grid.CurrentCell = grid.Rows[0].Cells[0]; }
        private static string GetAbilityName(AbilityAnalysisResult report) { ClientAbilityRecord client = report.ClientAbilityRows.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name)); if (client != null) return client.Name; IndexedStringRecord name = report.AbilityNameRows.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.NormalizedValue)); return name == null ? "Ability" : name.NormalizedValue; }
        private static string EffectName(AbilityAnalysisResult report, int effectId) { ClientEffectRecord row = report.ClientEffectRows.FirstOrDefault(x => x.EffectId == effectId); return row == null ? "Unknown effect" : NullToPlaceholder(row.Name); }
        private static string ResolveSubjectKind(string subjectKey) { if (string.IsNullOrWhiteSpace(subjectKey)) return string.Empty; int separator = subjectKey.IndexOf(':'); return separator <= 0 ? subjectKey : subjectKey.Substring(0, separator); }
        private static bool TryParseAbilityId(string subjectKey, out int abilityId)
        {
            abilityId = 0;
            if (string.IsNullOrWhiteSpace(subjectKey) || !subjectKey.StartsWith("Ability:", StringComparison.OrdinalIgnoreCase))
                return false;

            return int.TryParse(subjectKey.Substring("Ability:".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out abilityId);
        }
        private static bool TryParseAbilityIdFromSubjectKey(string subjectKey, out ushort abilityId)
        {
            abilityId = 0;
            if (string.IsNullOrWhiteSpace(subjectKey) || !subjectKey.StartsWith("Ability:", StringComparison.OrdinalIgnoreCase))
                return false;

            return ushort.TryParse(subjectKey.Substring("Ability:".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out abilityId);
        }
        private static bool TryParseEffectIdFromSubjectKey(string subjectKey, out int effectId)
        {
            effectId = 0;
            if (string.IsNullOrWhiteSpace(subjectKey) || !subjectKey.StartsWith("Effect:", StringComparison.OrdinalIgnoreCase))
                return false;

            return int.TryParse(subjectKey.Substring("Effect:".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out effectId);
        }
        private static string NullToPlaceholder(string value) { return string.IsNullOrWhiteSpace(value) ? "(none)" : value; }
        private static string TextOrNone(int value) { return value > 0 ? value.ToString(CultureInfo.InvariantCulture) : "(none)"; }

        private sealed class ConflictDomainRow { public string Domain { get; set; } public int PeakTriageScore { get; set; } public int HighSignalCount { get; set; } public int ConflictCount { get; set; } public int SubjectCount { get; set; } public int FactCount { get; set; } }
        private sealed class ConflictListRow { public string TriageBucket { get; set; } public int TriageScore { get; set; } public string TriageCategory { get; set; } public string ResolutionHint { get; set; } public string SubjectKind { get; set; } public string SubjectKey { get; set; } public string FactName { get; set; } public string Domain { get; set; } public string DistinctValues { get; set; } public int ClaimCount { get; set; } public ConflictRecord Conflict { get; set; } }
        private sealed class ConflictValueListRow { public string ValueText { get; set; } public string Meaning { get; set; } public string SourcesText { get; set; } public int ClaimCount { get; set; } public string Notes { get; set; } }
        private sealed class ConflictClaimListRow { public string NormalizedValue { get; set; } public string RawValue { get; set; } public string SourceFamily { get; set; } public string TableName { get; set; } public string FieldName { get; set; } public string Confidence { get; set; } public string SourcePath { get; set; } public string SourceLocation { get; set; } public string Notes { get; set; } }
        private sealed class TokenDefinitionListRow { public string ExampleToken { get; set; } public string FieldKey { get; set; } public string PlainEnglishMeaning { get; set; } public string Confidence { get; set; } public string ContextTagsText { get; set; } public string ExampleAbilityIdsText { get; set; } public string Notes { get; set; } }
        private sealed class DomainListRow { public string DomainKey { get; set; } public string DisplayName { get; set; } public string Confidence { get; set; } public string Canonicality { get; set; } public int ValueCount { get; set; } public int DistinctMeaningCount { get; set; } public int DuplicateMeaningCount { get; set; } public string SourceFilesText { get; set; } public string RecommendedUsage { get; set; } public string Notes { get; set; } }
        private sealed class DomainValueListRow { public string RawValue { get; set; } public string Meaning { get; set; } public string Confidence { get; set; } public string Source { get; set; } public string SourcePath { get; set; } public string SourceLocation { get; set; } public string Notes { get; set; } }
        private sealed class RequirementListRow { public ushort RequirementId { get; set; } public int RecordCount { get; set; } public int FieldCount { get; set; } public int DirectAbilityCount { get; set; } public int DirectComponentCount { get; set; } public int ParentRequirementCount { get; set; } public int ChildRequirementCount { get; set; } public string ContextTagsText { get; set; } public string SampleAbilitiesText { get; set; } public string SemanticSummary { get; set; } public string Notes { get; set; } }
        private sealed class RequirementRowListRow { public int RecordIndex { get; set; } public string ExtDataText { get; set; } public string SourcePath { get; set; } public string SourceLocation { get; set; } }
        private sealed class RequirementFieldListRow { public string FieldKey { get; set; } public int NonZeroCount { get; set; } public int DistinctValueCount { get; set; } public string SampleValuesText { get; set; } public string SemanticSummary { get; set; } public string Confidence { get; set; } public string Notes { get; set; } }
        private sealed class RequirementReferenceListRow { public string Direction { get; set; } public string SourceKind { get; set; } public ushort SourceId { get; set; } public string SourceLabel { get; set; } public string SourceField { get; set; } public ushort LinkedRequirementId { get; set; } public string RelatedAbilitiesText { get; set; } public string ContextTagsText { get; set; } public string SourcePath { get; set; } public string SourceLocation { get; set; } public string Notes { get; set; } }
        private sealed class CoverageListRow { public ushort AbilityId { get; set; } public string Name { get; set; } public string CoverageStatus { get; set; } public string EffectIdText { get; set; } public string HasClientCsvText { get; set; } public string HasClientBinText { get; set; } public string HasLocalizedNameText { get; set; } public string HasDescriptionTextText { get; set; } public string HasEffectTextText { get; set; } public string HasRootEffectRowText { get; set; } public int ComponentCount { get; set; } public int RequirementLinkCount { get; set; } public int RequirementRowCount { get; set; } public string SourcesText { get; set; } public string MissingText { get; set; } }
        private sealed class RemainingWorkAreaListRow { public string AreaKey { get; set; } public string Title { get; set; } public int ItemCount { get; set; } public int CriticalCount { get; set; } public int HighCount { get; set; } public int PeakScore { get; set; } public string PeakBucket { get; set; } public string Summary { get; set; } public string RecommendedNextStep { get; set; } }
        private sealed class RemainingWorkItemListRow { public int Rank { get; set; } public int GlobalRank { get; set; } public string PriorityBucket { get; set; } public int PriorityScore { get; set; } public string SubjectKind { get; set; } public string SubjectKey { get; set; } public string Title { get; set; } public string Summary { get; set; } public string Evidence { get; set; } public string RecommendedAction { get; set; } public ushort? ExampleAbilityId { get; set; } public string ExampleAbilityIdText { get; set; } public string ReferencePath { get; set; } public string ReferenceLocation { get; set; } }
        private sealed class OperationSchemaListRow { public uint OperationId { get; set; } public string OperationName { get; set; } public string PriorityText { get; set; } public int ComponentCount { get; set; } public int AbilityCount { get; set; } public string ContextTagsText { get; set; } public string LayoutVariantsText { get; set; } }
        private sealed class OperationFieldListRow { public string FieldKey { get; set; } public string TriageBucket { get; set; } public int TriageScore { get; set; } public int NonZeroCount { get; set; } public int DistinctValueCount { get; set; } public string SampleValuesText { get; set; } public string TokenRenderingsText { get; set; } public string SemanticSummary { get; set; } public string Confidence { get; set; } public string ContextTagsText { get; set; } public string Notes { get; set; } }
        private sealed class OperationAbilityListRow { public ushort AbilityId { get; set; } public string AbilityName { get; set; } public ushort ComponentId { get; set; } public int ComponentSlotIndex { get; set; } public string TriggerText { get; set; } public string ContextTagsText { get; set; } public string TextExcerpt { get; set; } public string SourcePath { get; set; } public string SourceLocation { get; set; } }
        private sealed class UnknownFieldListRow { public string TriageBucket { get; set; } public int TriageScore { get; set; } public string TriageNotes { get; set; } public string Confidence { get; set; } public string PriorityText { get; set; } public uint OperationId { get; set; } public string OperationName { get; set; } public string FieldKey { get; set; } public int NonZeroCount { get; set; } public int DistinctValueCount { get; set; } public string ContextTagsText { get; set; } public string SampleValuesText { get; set; } public string Notes { get; set; } public ComponentOperationSchemaRecord Operation { get; set; } }
        private sealed class UnknownValueListRow { public string RawValue { get; set; } public int ObservationCount { get; set; } public int DistinctComponentCount { get; set; } public int DistinctAbilityCount { get; set; } public string SampleComponentIdsText { get; set; } public string SampleAbilityIdsText { get; set; } public ComponentOperationFieldValueRecord Value { get; set; } }
        private sealed class UnknownCompanionListRow { public string FieldKey { get; set; } public string DominantValue { get; set; } public int CoveragePercent { get; set; } public int MatchCount { get; set; } public int ObservationCount { get; set; } public int DistinctValueCount { get; set; } public string SampleValuesText { get; set; } }
    }
}
