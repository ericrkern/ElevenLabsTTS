using System;
using System.Drawing;
using System.Windows.Forms;
using ElevenLabsTTS.Models;
using ElevenLabsTTS.Services;
using System.Media;
using NAudio.Wave;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace ElevenLabsTTS
{
    public partial class ConfigurationForm : Form
    {
        private TextBox apiKeyInput;
        private ComboBox voiceSelector;
        private ComboBox modelSelector;
        private ComboBox languageSelector;
        private TrackBar stabilitySlider;
        private TrackBar similarityBoostSlider;
        private TrackBar styleSlider;
        private CheckBox speakerBoostCheckbox;
        private TrackBar speedSlider;
        private TrackBar volumeSlider;
        private Label volumeLabel;
        private Label stabilityLabel;
        private Label similarityBoostLabel;
        private Label styleLabel;
        private Label speedLabel;
        private ComboBox outputFormatSelector;
        private Button loadVoicesButton;
        private Button previewButton;
        private Button stopPreviewButton;
        private Button saveButton;
        private Button cancelButton;
        private Label statusLabel;
        private Panel mainPanel;

        private Configuration _config;
        private ElevenLabsApi _api;
        private List<Voice> _voices;
        private IWavePlayer wavePlayer;
        private WaveStream waveStream;
        private bool isPlaying = false;
        private float currentVolume = 1.0f;

        public ConfigurationForm(Configuration config, ElevenLabsApi api)
        {
            InitializeComponent();
            this.Size = new Size(800, 900); // Increased form size
            this.MinimumSize = new Size(750, 800); // Increased minimum size
            _config = config;
            _api = api;
            InitializeUI();
        }

        private void InitializeComponent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;

            // Add your existing code here

            InitializeUI();
        }

        private void InitializeUI()
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };
            this.Controls.Add(mainPanel);

            int currentY = 30; // Initial top margin
            int labelWidth = 160;
            int controlSpacing = 15;
            int controlWidth = Math.Min(450, this.ClientSize.Width - 200);
            int verticalSpacing = 70; // Increased from 60 to 70
            int sectionSpacing = 50; // Increased from 40 to 50
            int leftMargin = 40;

            // API Key Section
            var apiKeyLabel = AddLabel("API Key:", leftMargin, currentY, labelWidth);
            apiKeyInput = AddTextBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth, true);
            currentY += verticalSpacing + sectionSpacing;

            // Voice Selection Section
            var voiceLabel = AddLabel("Voice:", leftMargin, currentY, labelWidth);
            voiceSelector = AddComboBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            currentY += verticalSpacing;

            loadVoicesButton = new Button
            {
                Text = "Load Voices",
                Location = new Point(leftMargin + labelWidth + controlSpacing, currentY),
                Size = new Size(140, 35)
            };
            loadVoicesButton.Click += LoadVoicesButton_Click;
            mainPanel.Controls.Add(loadVoicesButton);
            currentY += verticalSpacing + sectionSpacing;

            // Model Selection
            var modelLabel = AddLabel("Model:", leftMargin, currentY, labelWidth);
            modelSelector = AddComboBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            currentY += verticalSpacing + sectionSpacing;

            // Language Selection
            var languageLabel = AddLabel("Language:", leftMargin, currentY, labelWidth);
            languageSelector = AddComboBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            currentY += verticalSpacing + sectionSpacing;

            // Voice Settings Section
            var stabilityLabel = AddLabel("Stability:", leftMargin, currentY, labelWidth);
            stabilitySlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            this.stabilityLabel = AddValueLabel(leftMargin + labelWidth + controlSpacing + controlWidth + 10, currentY, "0");
            currentY += verticalSpacing;

            var similarityBoostLabel = AddLabel("Similarity Boost:", leftMargin, currentY, labelWidth);
            similarityBoostSlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            this.similarityBoostLabel = AddValueLabel(leftMargin + labelWidth + controlSpacing + controlWidth + 10, currentY, "0");
            currentY += verticalSpacing;

            var styleLabel = AddLabel("Style:", leftMargin, currentY, labelWidth);
            styleSlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            this.styleLabel = AddValueLabel(leftMargin + labelWidth + controlSpacing + controlWidth + 10, currentY, "0");
            currentY += verticalSpacing;

            var speakerBoostLabel = AddLabel("Speaker Boost:", leftMargin, currentY, labelWidth);
            speakerBoostCheckbox = new CheckBox
            {
                Location = new Point(leftMargin + labelWidth + controlSpacing, currentY + 5),
                AutoSize = true
            };
            mainPanel.Controls.Add(speakerBoostCheckbox);
            currentY += verticalSpacing + sectionSpacing;

            // Playback Settings Section
            var speedLabel = AddLabel("Speed:", leftMargin, currentY, labelWidth);
            speedSlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            this.speedLabel = AddValueLabel(leftMargin + labelWidth + controlSpacing + controlWidth + 10, currentY, "0");
            currentY += verticalSpacing;

            var volumeLabel = AddLabel("Volume:", leftMargin, currentY, labelWidth);
            volumeSlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            this.volumeLabel = AddValueLabel(leftMargin + labelWidth + controlSpacing + controlWidth + 10, currentY, "0");
            currentY += verticalSpacing + sectionSpacing;

            // Output Format Section
            var outputFormatLabel = AddLabel("Output Format:", leftMargin, currentY, labelWidth);
            outputFormatSelector = AddComboBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            currentY += verticalSpacing + sectionSpacing;

            // Preview Section
            previewButton = new Button
            {
                Text = "Preview Voice",
                Location = new Point(leftMargin, currentY),
                Size = new Size(140, 35)
            };
            previewButton.Click += PreviewButton_Click;
            mainPanel.Controls.Add(previewButton);

            stopPreviewButton = new Button
            {
                Text = "Stop Preview",
                Location = new Point(leftMargin + 160, currentY),
                Size = new Size(140, 35),
                Enabled = false
            };
            stopPreviewButton.Click += StopPreviewButton_Click;
            mainPanel.Controls.Add(stopPreviewButton);
            currentY += verticalSpacing + sectionSpacing;

            // Status Label
            statusLabel = new Label
            {
                Location = new Point(leftMargin, currentY),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            mainPanel.Controls.Add(statusLabel);
            currentY += verticalSpacing;

            // Save and Cancel Buttons at the bottom
            int buttonY = Math.Max(currentY, this.ClientSize.Height - 100);
            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(this.ClientSize.Width - 300, buttonY),
                Size = new Size(120, 35),
                DialogResult = DialogResult.OK
            };
            saveButton.Click += SaveButton_Click;
            mainPanel.Controls.Add(saveButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(this.ClientSize.Width - 160, buttonY),
                Size = new Size(120, 35),
                DialogResult = DialogResult.Cancel
            };
            mainPanel.Controls.Add(cancelButton);

            // Initialize control values
            InitializeControlValues();
            
            // Handle form resize
            this.Resize += ConfigurationForm_Resize;
        }

        private void ConfigurationForm_Resize(object sender, EventArgs e)
        {
            if (saveButton != null && cancelButton != null)
            {
                // Update save and cancel button positions when form is resized
                saveButton.Location = new Point(this.ClientSize.Width - 300, Math.Max(mainPanel.Controls.Cast<Control>().Max(c => c.Bottom) + 20, this.ClientSize.Height - 100));
                cancelButton.Location = new Point(this.ClientSize.Width - 160, saveButton.Location.Y);
            }
        }

        private Label AddLabel(string text, int x, int y, int width)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y + 8),
                AutoSize = true,
                AutoEllipsis = true,
                MaximumSize = new Size(width, 0), // Setting maxWidth but no maxHeight allows vertical auto-sizing
                MinimumSize = new Size(width, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(label);
            return label;
        }

        private Label AddValueLabel(int x, int y, string initialValue)
        {
            var label = new Label
            {
                Text = initialValue,
                Location = new Point(x, y),
                AutoSize = true,
                MinimumSize = new Size(60, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(label);
            return label;
        }

        private TextBox AddTextBox(int x, int y, int width, bool isPassword = false)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 50),
                Font = new Font("Segoe UI", 10F),
                Padding = new Padding(5, 12, 5, 12),
                BorderStyle = BorderStyle.FixedSingle
            };
            if (isPassword)
            {
                textBox.UseSystemPasswordChar = true;
            }
            mainPanel.Controls.Add(textBox);
            return textBox;
        }

        private ComboBox AddComboBox(int x, int y, int width)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 35),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(comboBox);
            return comboBox;
        }

        private TrackBar AddTrackBar(int x, int y, int width)
        {
            var trackBar = new TrackBar
            {
                Location = new Point(x, y),
                Size = new Size(width, 50),
                Minimum = 0,
                Maximum = 100,
                Value = 50
            };
            mainPanel.Controls.Add(trackBar);
            return trackBar;
        }

        private CheckBox AddCheckBox(int x, int y, int width, string text)
        {
            var checkBox = new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            };
            mainPanel.Controls.Add(checkBox);
            return checkBox;
        }

        private Button AddButton(int x, int y, int width, string text)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 35),
                DialogResult = DialogResult.OK
            };
            mainPanel.Controls.Add(button);
            return button;
        }

        private void InitializeControlValues()
        {
            // Initialize model selector
            modelSelector.Items.Clear();
            modelSelector.Items.AddRange(new string[] {
                "eleven_multilingual_v2",
                "eleven_monolingual_v1",
                "eleven_turbo_v2"
            });

            // Initialize language selector
            languageSelector.Items.Clear();
            foreach (var language in Configuration.SupportedLanguages.Keys)
            {
                languageSelector.Items.Add(language);
            }

            // Initialize output format selector
            outputFormatSelector.Items.Clear();
            outputFormatSelector.Items.AddRange(new string[] {
                "mp3_44100_128",
                "mp3_44100_192",
                "pcm_16000",
                "pcm_22050",
                "pcm_24000",
                "pcm_44100"
            });

            // Load configuration
            LoadConfiguration();

            // Configure trackbar events after values are set
            stabilitySlider.ValueChanged += (s, e) => this.stabilityLabel.Text = stabilitySlider.Value.ToString();
            similarityBoostSlider.ValueChanged += (s, e) => this.similarityBoostLabel.Text = similarityBoostSlider.Value.ToString();
            styleSlider.ValueChanged += (s, e) => this.styleLabel.Text = styleSlider.Value.ToString();
            speedSlider.ValueChanged += (s, e) => this.speedLabel.Text = speedSlider.Value.ToString();
            volumeSlider.ValueChanged += (s, e) => this.volumeLabel.Text = volumeSlider.Value.ToString();

            // Update labels with initial values
            this.stabilityLabel.Text = stabilitySlider.Value.ToString();
            this.similarityBoostLabel.Text = similarityBoostSlider.Value.ToString();
            this.styleLabel.Text = styleSlider.Value.ToString();
            this.speedLabel.Text = speedSlider.Value.ToString();
            this.volumeLabel.Text = volumeSlider.Value.ToString();
        }

        private void LoadConfiguration()
        {
            apiKeyInput.Text = _config.ApiKey;
            
            // Set model
            if (modelSelector.Items.Contains(_config.SelectedModel))
            {
                modelSelector.SelectedItem = _config.SelectedModel;
            }
            else if (modelSelector.Items.Count > 0)
            {
                modelSelector.SelectedIndex = 0;
            }
            
            // Set language
            if (!string.IsNullOrEmpty(_config.Language) && languageSelector.Items.Contains(_config.Language))
            {
                languageSelector.SelectedItem = _config.Language;
            }
            else if (languageSelector.Items.Count > 0)
            {
                languageSelector.SelectedItem = "English"; // Default to English
            }
            
            // Set output format
            if (outputFormatSelector.Items.Contains(_config.OutputFormat))
            {
                outputFormatSelector.SelectedItem = _config.OutputFormat;
            }
            else if (outputFormatSelector.Items.Count > 0)
            {
                outputFormatSelector.SelectedIndex = 0;
            }
            
            // Set slider values
            stabilitySlider.Value = _config.Stability;
            similarityBoostSlider.Value = _config.SimilarityBoost;
            styleSlider.Value = _config.Style;
            speakerBoostCheckbox.Checked = _config.UseSpeakerBoost;
            speedSlider.Value = _config.Speed;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            _config.ApiKey = apiKeyInput.Text;
            _config.SelectedModel = modelSelector.SelectedItem?.ToString() ?? "eleven_multilingual_v2";
            _config.Language = languageSelector.SelectedItem?.ToString() ?? "English";
            _config.Stability = stabilitySlider.Value;
            _config.SimilarityBoost = similarityBoostSlider.Value;
            _config.Style = styleSlider.Value;
            _config.UseSpeakerBoost = speakerBoostCheckbox.Checked;
            _config.Speed = speedSlider.Value;
            _config.OutputFormat = outputFormatSelector.SelectedItem?.ToString() ?? "mp3_44100_192";
            
            _config.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
} 