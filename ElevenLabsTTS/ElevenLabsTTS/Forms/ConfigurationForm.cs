using System.Windows.Forms;
using ElevenLabsTTS.Models;
using ElevenLabsTTS.Services;
using System.Media;
using NAudio.Wave;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ElevenLabsTTS
{
    public partial class ConfigurationForm : Form
    {
        private TextBox apiKeyInput;
        private ComboBox voiceSelector;
        private ComboBox modelSelector;
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

        public ConfigurationForm()
        {
            InitializeComponent();
            InitializeUI();
            _ = LoadConfiguration();
        }

        public ConfigurationForm(Configuration config)
        {
            InitializeComponent();
            InitializeUI();
            _config = config;
            if (!string.IsNullOrEmpty(_config?.ApiKey))
            {
                _api = new ElevenLabsApi(_config.ApiKey);
            }
            _ = LoadConfigurationFromInstance();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuration";
            this.Size = new Size(750, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(700, 600);
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.FormClosing += ConfigurationForm_FormClosing;
            this.Resize += ConfigurationForm_Resize;

            // Add main panel with scroll
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            this.Controls.Add(mainPanel);
        }

        private void ConfigurationForm_Resize(object sender, EventArgs e)
        {
            // Update control widths when form is resized
            int controlWidth = Math.Min(450, this.ClientSize.Width - 200);
            foreach (Control control in mainPanel.Controls)
            {
                if (control is ComboBox || control is TextBox)
                {
                    control.Width = controlWidth;
                }
                else if (control is TrackBar)
                {
                    control.Width = controlWidth - 50;
                }
            }
        }

        private void InitializeUI()
        {
            int currentY = 30; // Initial top margin
            int labelWidth = 160;
            int controlSpacing = 25;
            int controlWidth = Math.Min(450, this.ClientSize.Width - 200);
            int verticalSpacing = 60; // Increased from 50 to accommodate larger sliders
            int sectionSpacing = 60; // Increased from 50
            int leftMargin = 40;
            int textBoxHeight = 35;
            
            // API Key
            AddLabel("API Key:", leftMargin, currentY, labelWidth);
            apiKeyInput = AddTextBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth, true);
            currentY += verticalSpacing;

            // Load Voices Button
            loadVoicesButton = new Button
            {
                Text = "Load Voices",
                Location = new Point(leftMargin + labelWidth + controlSpacing, currentY),
                Size = new Size(140, textBoxHeight) // Made button bigger
            };
            loadVoicesButton.Click += LoadVoicesButton_Click;
            mainPanel.Controls.Add(loadVoicesButton);
            currentY += verticalSpacing;

            // Voice Selector
            AddLabel("Voice:", leftMargin, currentY, labelWidth);
            voiceSelector = AddComboBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            currentY += verticalSpacing;

            // Preview and Stop Buttons
            previewButton = new Button
            {
                Text = "Preview Voice",
                Location = new Point(leftMargin + labelWidth + controlSpacing, currentY),
                Size = new Size(140, textBoxHeight),
                Enabled = false
            };
            previewButton.Click += PreviewButton_Click;
            mainPanel.Controls.Add(previewButton);

            stopPreviewButton = new Button
            {
                Text = "Stop",
                Location = new Point(leftMargin + labelWidth + controlSpacing + 150, currentY),
                Size = new Size(120, textBoxHeight),
                Enabled = false
            };
            stopPreviewButton.Click += StopPreviewButton_Click;
            mainPanel.Controls.Add(stopPreviewButton);
            currentY += verticalSpacing;

            // Model Selector
            AddLabel("Model:", leftMargin, currentY, labelWidth);
            modelSelector = AddComboBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            PopulateModelSelector();
            currentY += verticalSpacing;

            // Output Format
            AddLabel("Output Format:", leftMargin, currentY, labelWidth);
            outputFormatSelector = AddComboBox(leftMargin + labelWidth + controlSpacing, currentY, controlWidth);
            PopulateOutputFormatSelector();
            currentY += verticalSpacing;

            // Volume Control
            AddLabel("Volume:", leftMargin, currentY, labelWidth);
            volumeSlider = new AccessibleTrackBar
            {
                Location = new Point(leftMargin + labelWidth + controlSpacing, currentY),
                Size = new Size(controlWidth - 50, 65), // Increased height for better touch targets
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 5, // More frequent ticks for better visual feedback
                TickStyle = TickStyle.Both,
                LargeChange = 10, // Larger steps with keyboard/button navigation
                SmallChange = 1,
                AutoSize = false
            };
            // Set the control to allow focus for keyboard accessibility
            volumeSlider.TabStop = true;
            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            mainPanel.Controls.Add(volumeSlider);

            volumeLabel = new Label
            {
                Text = "100%",
                Location = new Point(volumeSlider.Right + 10, currentY + 8),
                Size = new Size(60, 35), // Wider to fit values better
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), // Larger font
                TextAlign = ContentAlignment.MiddleLeft // Better alignment
            };
            mainPanel.Controls.Add(volumeLabel);
            currentY += verticalSpacing + 30; // Added extra spacing due to larger slider height

            // Voice Boost Parameters Section Header
            AddLabel("Voice Boost Parameters", leftMargin, currentY, controlWidth);
            currentY += sectionSpacing;

            // Stability Slider
            AddLabel("Stability:", leftMargin, currentY, labelWidth);
            stabilitySlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth - 50);
            stabilityLabel = AddValueLabel(stabilitySlider.Right + 10, currentY + 8, "50%");
            stabilitySlider.ValueChanged += (s, e) => stabilityLabel.Text = $"{stabilitySlider.Value}%";
            currentY += verticalSpacing;

            // Similarity Boost Slider
            AddLabel("Similarity:", leftMargin, currentY, labelWidth);
            similarityBoostSlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth - 50);
            similarityBoostLabel = AddValueLabel(similarityBoostSlider.Right + 10, currentY + 8, "75%");
            similarityBoostSlider.ValueChanged += (s, e) => similarityBoostLabel.Text = $"{similarityBoostSlider.Value}%";
            currentY += verticalSpacing;

            // Style Slider
            AddLabel("Style:", leftMargin, currentY, labelWidth);
            styleSlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth - 50);
            styleLabel = AddValueLabel(styleSlider.Right + 10, currentY + 8, "0%");
            styleSlider.ValueChanged += (s, e) => styleLabel.Text = $"{styleSlider.Value}%";
            currentY += verticalSpacing;

            // Speaker Boost Checkbox
            AddLabel("Speaker Boost:", leftMargin, currentY, labelWidth);
            speakerBoostCheckbox = new CheckBox
            {
                Text = "Enable",
                Location = new Point(leftMargin + labelWidth + controlSpacing, currentY + 5),
                Size = new Size(100, textBoxHeight),
                Checked = true
            };
            mainPanel.Controls.Add(speakerBoostCheckbox);
            currentY += verticalSpacing;

            // Speed Slider
            AddLabel("Speed:", leftMargin, currentY, labelWidth);
            speedSlider = AddTrackBar(leftMargin + labelWidth + controlSpacing, currentY, controlWidth - 50);
            speedLabel = AddValueLabel(speedSlider.Right + 10, currentY + 8, "50%");
            speedSlider.ValueChanged += (s, e) => speedLabel.Text = $"{speedSlider.Value}%";
            currentY += verticalSpacing + 30;

            // Status Label
            statusLabel = new Label
            {
                Location = new Point(leftMargin, currentY),
                Size = new Size(620, textBoxHeight),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            mainPanel.Controls.Add(statusLabel);
            currentY += verticalSpacing;

            // Save and Cancel Buttons at the top
            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(leftMargin, currentY),
                Size = new Size(120, textBoxHeight),
                DialogResult = DialogResult.OK
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(leftMargin + 130, currentY),
                Size = new Size(120, textBoxHeight),
                DialogResult = DialogResult.Cancel
            };

            mainPanel.Controls.AddRange(new Control[] { saveButton, cancelButton });
            currentY += verticalSpacing;

            // Voice selector change event
            voiceSelector.SelectedIndexChanged += VoiceSelector_SelectedIndexChanged;

            // Set panel's virtual size
            mainPanel.AutoScrollMinSize = new Size(0, currentY + 100);
        }

        private Label AddLabel(string text, int x, int y, int width)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y + 5),
                Size = new Size(width, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            mainPanel.Controls.Add(label);
            return label;
        }

        private TextBox AddTextBox(int x, int y, int width, bool isPassword = false)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 35),
                Font = new Font("Segoe UI", 9F)
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
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(comboBox);
            return comboBox;
        }

        private TrackBar AddTrackBar(int x, int y, int width)
        {
            var trackBar = new AccessibleTrackBar
            {
                Location = new Point(x, y),
                Size = new Size(width, 65), // Increased height for better touch targets
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                TickFrequency = 5, // More frequent ticks for better visual feedback
                TickStyle = TickStyle.Both,
                LargeChange = 10, // Larger steps with keyboard/button navigation
                SmallChange = 1,
                AutoSize = false
            };
            
            // Set the control to allow focus for keyboard accessibility
            trackBar.TabStop = true;
            
            mainPanel.Controls.Add(trackBar);
            return trackBar;
        }

        private Label AddValueLabel(int x, int y, string initialValue)
        {
            var label = new Label
            {
                Text = initialValue,
                Location = new Point(x, y),
                Size = new Size(60, 35), // Wider to fit values with % symbol better
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), // Larger font
                TextAlign = ContentAlignment.MiddleLeft // Better text alignment
            };
            mainPanel.Controls.Add(label);
            return label;
        }

        private void PopulateModelSelector()
        {
            modelSelector.Items.AddRange(new string[] {
                "eleven_multilingual_v2",
                "eleven_flash_v2_5",
                "eleven_turbo_v2_5"
            });
            modelSelector.SelectedIndex = 0;
        }

        private void PopulateOutputFormatSelector()
        {
            var formats = new Dictionary<string, string>
            {
                { "mp3_44100_192", "MP3 - 44.1kHz 192kbps" },
                { "mp3_44100_128", "MP3 - 44.1kHz 128kbps" },
                { "mp3_44100_96", "MP3 - 44.1kHz 96kbps" },
                { "mp3_44100_64", "MP3 - 44.1kHz 64kbps" },
                { "pcm_48000", "WAV - 48kHz PCM" },
                { "pcm_44100", "WAV - 44.1kHz PCM" },
                { "pcm_24000", "WAV - 24kHz PCM" }
            };

            outputFormatSelector.DisplayMember = "Value";
            outputFormatSelector.ValueMember = "Key";
            outputFormatSelector.DataSource = new BindingSource(formats, null);
            outputFormatSelector.SelectedValue = _config?.OutputFormat ?? "mp3_44100_192";
        }

        private async Task LoadConfiguration()
        {
            try
            {
                _config = Configuration.Load();
                await LoadConfigurationFromInstance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error loading configuration.";
            }
        }

        private async Task LoadConfigurationFromInstance()
        {
            try
            {
                // Load values into UI
                apiKeyInput.Text = _config.ApiKey;
                modelSelector.SelectedItem = _config.SelectedModel;
                stabilitySlider.Value = _config.Stability;
                similarityBoostSlider.Value = _config.SimilarityBoost;
                styleSlider.Value = _config.Style;
                speakerBoostCheckbox.Checked = _config.UseSpeakerBoost;
                speedSlider.Value = _config.Speed;

                // Update labels
                stabilityLabel.Text = $"{_config.Stability}%";
                similarityBoostLabel.Text = $"{_config.SimilarityBoost}%";
                styleLabel.Text = $"{_config.Style}%";
                speedLabel.Text = $"{_config.Speed}%";

                // Set output format
                if (outputFormatSelector.Items.Count > 0)
                {
                    var formatItem = ((BindingSource)outputFormatSelector.DataSource)
                        .Cast<KeyValuePair<string, string>>()
                        .FirstOrDefault(x => x.Key == _config.OutputFormat);
                    
                    if (!EqualityComparer<KeyValuePair<string, string>>.Default.Equals(formatItem, default))
                    {
                        outputFormatSelector.SelectedItem = formatItem;
                    }
                }

                if (!string.IsNullOrEmpty(_config.ApiKey))
                {
                    _api = new ElevenLabsApi(_config.ApiKey);
                    await LoadVoices();
                }

                statusLabel.Text = "Configuration loaded successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error loading configuration.";
            }
        }

        private void VolumeSlider_ValueChanged(object sender, EventArgs e)
        {
            currentVolume = volumeSlider.Value / 100f;
            volumeLabel.Text = $"{volumeSlider.Value}%";

            // Update volume of currently playing audio
            if (wavePlayer != null && isPlaying)
            {
                if (wavePlayer is WaveOutEvent waveOut)
                {
                    waveOut.Volume = currentVolume;
                }
            }
        }

        private async void LoadVoicesButton_Click(object sender, EventArgs e)
        {
            await LoadVoices();
        }

        private async Task LoadVoices()
        {
            try
            {
                if (string.IsNullOrEmpty(apiKeyInput.Text))
                {
                    MessageBox.Show("Please enter an API key first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                loadVoicesButton.Enabled = false;
                statusLabel.Text = "Loading voices...";
                
                // Always create a new API instance with the current API key
                _api = new ElevenLabsApi(apiKeyInput.Text);
                
                _voices = await _api.GetVoicesAsync();
                
                voiceSelector.Items.Clear();
                foreach (var voice in _voices)
                {
                    voiceSelector.Items.Add(voice);
                }
                
                // Select previously configured voice if it exists
                if (!string.IsNullOrEmpty(_config?.SelectedVoiceId))
                {
                    var savedVoice = _voices.FirstOrDefault(v => v.Id == _config.SelectedVoiceId);
                    if (savedVoice != null)
                    {
                        voiceSelector.SelectedItem = savedVoice;
                    }
                }

                statusLabel.Text = $"Loaded {_voices.Count} voices.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading voices: {ex.Message}\n\nPlease make sure your API key is correct.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error loading voices.";
            }
            finally
            {
                loadVoicesButton.Enabled = true;
            }
        }

        private void VoiceSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            previewButton.Enabled = voiceSelector.SelectedItem != null;
            if (!previewButton.Enabled)
            {
                stopPreviewButton.Enabled = false;
                StopPreview();
            }
        }

        private void StopPreviewButton_Click(object sender, EventArgs e)
        {
            StopPreview();
            statusLabel.Text = "Preview stopped.";
        }

        private async void PreviewButton_Click(object sender, EventArgs e)
        {
            if (voiceSelector.SelectedItem is not Voice selectedVoice)
                return;

            try
            {
                StopPreview();

                previewButton.Enabled = false;
                stopPreviewButton.Enabled = false;
                statusLabel.Text = "Generating preview...";
                Cursor = Cursors.WaitCursor;

                var stability = stabilitySlider.Value / 100.0;
                var speed = speedSlider.Value / 100.0;

                var audioData = await _api.TextToSpeechAsync(
                    "Hello! This is a preview of how I sound.",
                    selectedVoice.Id,
                    modelSelector.SelectedItem?.ToString() ?? "Standard",
                    stability,
                    speed
                );

                // Create temporary file
                var tempFile = Path.GetTempFileName();
                await File.WriteAllBytesAsync(tempFile, audioData);

                // Play the audio
                waveStream = new MediaFoundationReader(tempFile);
                wavePlayer = new WaveOutEvent();
                wavePlayer.Init(waveStream);
                
                // Set initial volume
                if (wavePlayer is WaveOutEvent waveOut)
                {
                    waveOut.Volume = currentVolume;
                }

                wavePlayer.PlaybackStopped += (s, args) => 
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        isPlaying = false;
                        previewButton.Enabled = true;
                        stopPreviewButton.Enabled = false;
                        statusLabel.Text = "Preview finished.";
                        StopPreview();
                        File.Delete(tempFile);
                    }));
                };
                
                wavePlayer.Play();
                isPlaying = true;
                stopPreviewButton.Enabled = true;
                statusLabel.Text = "Playing preview...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error previewing voice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error previewing voice.";
                previewButton.Enabled = true;
                stopPreviewButton.Enabled = false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void StopPreview()
        {
            if (wavePlayer != null)
            {
                if (isPlaying)
                {
                    wavePlayer.Stop();
                }
                wavePlayer.Dispose();
                wavePlayer = null;
            }

            if (waveStream != null)
            {
                waveStream.Dispose();
                waveStream = null;
            }

            isPlaying = false;
            if (previewButton != null && !this.IsDisposed)
            {
                previewButton.Enabled = voiceSelector.SelectedItem != null;
                stopPreviewButton.Enabled = false;
            }
        }

        private void ConfigurationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopPreview();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKeyInput.Text))
                {
                    MessageBox.Show("Please enter an API key.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedVoice = voiceSelector.SelectedItem as Voice;
                if (selectedVoice == null)
                {
                    MessageBox.Show("Please select a voice.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _config.ApiKey = apiKeyInput.Text;
                _config.SelectedVoiceId = selectedVoice.Id;
                _config.SelectedVoiceName = selectedVoice.Name;
                _config.SelectedModel = modelSelector.SelectedItem?.ToString() ?? "eleven_multilingual_v2";
                _config.Stability = stabilitySlider.Value;
                _config.SimilarityBoost = similarityBoostSlider.Value;
                _config.Style = styleSlider.Value;
                _config.UseSpeakerBoost = speakerBoostCheckbox.Checked;
                _config.Speed = speedSlider.Value;
                _config.OutputFormat = ((KeyValuePair<string, string>)outputFormatSelector.SelectedItem).Key;
                
                statusLabel.Text = "Saving configuration...";
                _config.Save();
                
                statusLabel.Text = "Configuration saved successfully.";
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error saving configuration.";
            }
        }

        // Custom TrackBar with improved accessibility features
        private class AccessibleTrackBar : TrackBar
        {
            public AccessibleTrackBar() : base()
            {
                SetStyle(ControlStyles.UserPaint, true);
                DoubleBuffered = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                
                // Make the slider thumb larger and more visible
                // This code draws a border around the control to make its boundaries clear
                using (var pen = new Pen(Color.FromArgb(80, 80, 80), 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }
        }
    }
} 