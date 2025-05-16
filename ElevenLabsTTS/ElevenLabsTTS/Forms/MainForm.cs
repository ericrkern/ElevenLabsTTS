using System.Windows.Forms;
using ElevenLabsTTS.Models;
using ElevenLabsTTS.Services;
using NAudio.Wave;

namespace ElevenLabsTTS
{
    public partial class MainForm : Form
    {
        private TextBox textInput;
        private Button speakButton;
        private Button saveAudioButton;
        private Button copyButton;
        private Button stopButton;
        private Button configButton;
        private Button clearButton;
        private Label statusLabel;

        private Configuration _config;
        private ElevenLabsApi _api;
        private IWavePlayer wavePlayer;
        private WaveStream waveStream;
        private bool isPlaying = false;
        private string currentTempFile = null;
        private byte[] currentAudioData = null;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadConfiguration();
        }

        private void InitializeComponent()
        {
            this.Text = "Eleven Labs TTS";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeUI()
        {
            // Text input
            textInput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 90),
                Size = new Size(740, 330),
                Font = new Font("Segoe UI", 12F)
            };

            // Clear button
            clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(20, 20),
                Size = new Size(150, 60),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            clearButton.Click += ClearButton_Click;

            // Speak button
            speakButton = new Button
            {
                Text = "Speak",
                Location = new Point(20, 440),
                Size = new Size(150, 60),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            speakButton.Click += SpeakButton_Click;

            // Stop button
            stopButton = new Button
            {
                Text = "Stop",
                Location = new Point(180, 440),
                Size = new Size(150, 60),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true,
                Enabled = false
            };
            stopButton.Click += StopButton_Click;

            // Save Audio button
            saveAudioButton = new Button
            {
                Text = "Save Audio",
                Location = new Point(340, 440),
                Size = new Size(150, 60),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true,
                Enabled = false
            };
            saveAudioButton.Click += SaveAudioButton_Click;

            // Copy to Clipboard button
            copyButton = new Button
            {
                Text = "Copy",
                Location = new Point(500, 440),
                Size = new Size(150, 60),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true,
                Enabled = false,
                UseMnemonic = false
            };
            copyButton.Click += CopyButton_Click;

            // Config button
            configButton = new Button
            {
                Text = "Configuration",
                Location = new Point(660, 440),
                Size = new Size(150, 60),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            configButton.Click += ConfigButton_Click;

            // Status label
            statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(20, 520),
                Size = new Size(910, 60),
                Font = new Font("Segoe UI", 10F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.AddRange(new Control[]
            {
                textInput, 
                clearButton,
                speakButton,
                stopButton,
                saveAudioButton,
                copyButton,
                configButton,
                statusLabel 
            });
        }

        private void LoadConfiguration()
        {
            try
            {
                _config = Configuration.Load();
                if (!string.IsNullOrEmpty(_config.ApiKey))
                {
                    _api = new ElevenLabsApi(_config.ApiKey);
                    if (!string.IsNullOrEmpty(_config.SelectedVoiceId))
                    {
                        speakButton.Enabled = true;
                        statusLabel.Text = "Ready to convert text to speech.";
                    }
                    else
                    {
                        speakButton.Enabled = false;
                        statusLabel.Text = "Please select a voice in Configuration.";
                    }
                }
                else
                {
                    speakButton.Enabled = false;
                    statusLabel.Text = "Please configure API key and voice in Configuration.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error loading configuration.";
                speakButton.Enabled = false;
            }
        }

        private async void SpeakButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textInput.Text))
            {
                MessageBox.Show("Please enter some text to convert to speech.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                StopPlayback();
                
                speakButton.Enabled = false;
                configButton.Enabled = false;
                saveAudioButton.Enabled = false;
                copyButton.Enabled = false;
                statusLabel.Text = "Converting text to speech...";
                Cursor = Cursors.WaitCursor;

                var stability = _config.Stability / 100.0;
                var speed = _config.Speed / 100.0;
                
                // Get language code from the configuration
                string languageCode = "en"; // Default to English
                if (!string.IsNullOrEmpty(_config.Language) && 
                    Configuration.SupportedLanguages.TryGetValue(_config.Language, out var code))
                {
                    languageCode = code;
                }

                currentAudioData = await _api.TextToSpeechAsync(
                    textInput.Text,
                    _config.SelectedVoiceId,
                    _config.SelectedModel,
                    stability,
                    speed,
                    languageCode // Pass the language code
                );

                // Create temporary file for playback
                if (currentTempFile != null && File.Exists(currentTempFile))
                {
                    File.Delete(currentTempFile);
                }
                
                currentTempFile = Path.GetTempFileName();
                await File.WriteAllBytesAsync(currentTempFile, currentAudioData);

                // Play the audio
                waveStream = new MediaFoundationReader(currentTempFile);
                wavePlayer = new WaveOutEvent();
                wavePlayer.Init(waveStream);

                wavePlayer.PlaybackStopped += (s, args) =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        isPlaying = false;
                        speakButton.Enabled = true;
                        configButton.Enabled = true;
                        stopButton.Enabled = false;
                        saveAudioButton.Enabled = true;
                        copyButton.Enabled = true;
                        statusLabel.Text = "Playback finished.";
                    }));
                };

                wavePlayer.Play();
                isPlaying = true;
                stopButton.Enabled = true;
                saveAudioButton.Enabled = true;
                copyButton.Enabled = true;
                statusLabel.Text = "Playing audio...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting text to speech: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error converting text to speech.";
            }
            finally
            {
                speakButton.Enabled = true;
                configButton.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopPlayback();
            statusLabel.Text = "Playback stopped.";
        }

        private async void SaveAudioButton_Click(object sender, EventArgs e)
        {
            if (currentAudioData == null)
            {
                MessageBox.Show("No audio data available to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Increment the file number
                _config.LastFileNumber++;
                
                // Generate file name with voice name and number
                string voiceName = _config.SelectedVoiceName.Replace(" ", "");
                if (string.IsNullOrEmpty(voiceName)) voiceName = "voice";
                
                string fileName = $"{voiceName}_{_config.LastFileNumber:D3}.mp3";

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = GetFileFilter(),
                    FileName = fileName
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, currentAudioData);
                    _config.Save(); // Save to update the LastFileNumber
                    statusLabel.Text = "Audio file saved successfully.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving audio file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error saving audio file.";
            }
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            if (currentAudioData == null)
            {
                MessageBox.Show("No audio data available to copy.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Create a temporary file with mp3 extension
                string tempFile = Path.Combine(
                    Path.GetTempPath(),
                    $"voice_{DateTime.Now:yyyyMMddHHmmss}.mp3"
                );
                
                File.WriteAllBytes(tempFile, currentAudioData);

                // Create a collection of files to copy
                var fileCollection = new System.Collections.Specialized.StringCollection();
                fileCollection.Add(tempFile);

                // Copy the file path to clipboard
                Clipboard.SetFileDropList(fileCollection);
                
                statusLabel.Text = "Audio copied to clipboard. You can now paste it into a message.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error copying to clipboard.";
            }
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            StopPlayback();
            
            var configForm = new ConfigurationForm(_config);
            if (configForm.ShowDialog() == DialogResult.OK)
            {
                LoadConfiguration();
            }
        }

        private void StopPlayback()
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
            stopButton.Enabled = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopPlayback();
            
            if (currentTempFile != null && File.Exists(currentTempFile))
            {
                try
                {
                    File.Delete(currentTempFile);
                }
                catch { }
            }
        }

        private string GetFileFilter()
        {
            return _config.OutputFormat.StartsWith("mp3") 
                ? "MP3 Files (*.mp3)|*.mp3" 
                : "WAV Files (*.wav)|*.wav";
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            textInput.Text = string.Empty;
            statusLabel.Text = "Text cleared";
        }
    }
} 