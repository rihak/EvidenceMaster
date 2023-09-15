namespace EvidenceMaster
{
    partial class FormHome
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            comboBoxCI = new ComboBox();
            textBoxReference = new TextBox();
            listViewContents = new ListView();
            buttonGo = new Button();
            buttonReset = new Button();
            buttonHelp = new Button();
            progressBarCreation = new ProgressBar();
            SuspendLayout();
            // 
            // comboBoxCI
            // 
            comboBoxCI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxCI.AutoCompleteMode = AutoCompleteMode.Suggest;
            comboBoxCI.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxCI.FormattingEnabled = true;
            comboBoxCI.Location = new Point(12, 12);
            comboBoxCI.Name = "comboBoxCI";
            comboBoxCI.Size = new Size(330, 23);
            comboBoxCI.TabIndex = 2;
            // 
            // textBoxReference
            // 
            textBoxReference.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxReference.Location = new Point(12, 41);
            textBoxReference.Name = "textBoxReference";
            textBoxReference.PlaceholderText = "Reference";
            textBoxReference.Size = new Size(330, 23);
            textBoxReference.TabIndex = 4;
            // 
            // listViewContents
            // 
            listViewContents.AllowDrop = true;
            listViewContents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewContents.Location = new Point(12, 70);
            listViewContents.Name = "listViewContents";
            listViewContents.Size = new Size(330, 246);
            listViewContents.TabIndex = 5;
            listViewContents.UseCompatibleStateImageBehavior = false;
            listViewContents.View = View.List;
            listViewContents.DragDrop += listViewContents_DragDrop;
            listViewContents.DragEnter += listViewContents_DragEnter;
            listViewContents.KeyDown += listViewContents_KeyDown;
            // 
            // buttonGo
            // 
            buttonGo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonGo.BackColor = Color.LightSteelBlue;
            buttonGo.Location = new Point(267, 351);
            buttonGo.Name = "buttonGo";
            buttonGo.Size = new Size(75, 23);
            buttonGo.TabIndex = 6;
            buttonGo.Text = "Go";
            buttonGo.UseVisualStyleBackColor = false;
            buttonGo.Click += buttonGo_Click;
            // 
            // buttonReset
            // 
            buttonReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonReset.BackColor = Color.Salmon;
            buttonReset.Location = new Point(186, 351);
            buttonReset.Name = "buttonReset";
            buttonReset.Size = new Size(75, 23);
            buttonReset.TabIndex = 7;
            buttonReset.Text = "Reset";
            buttonReset.UseVisualStyleBackColor = false;
            buttonReset.Click += buttonReset_Click;
            // 
            // buttonHelp
            // 
            buttonHelp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonHelp.Location = new Point(12, 351);
            buttonHelp.Name = "buttonHelp";
            buttonHelp.Size = new Size(75, 23);
            buttonHelp.TabIndex = 8;
            buttonHelp.Text = "Help";
            buttonHelp.UseVisualStyleBackColor = true;
            buttonHelp.Click += buttonHelp_Click;
            // 
            // progressBarCreation
            // 
            progressBarCreation.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBarCreation.Location = new Point(12, 322);
            progressBarCreation.Name = "progressBarCreation";
            progressBarCreation.Size = new Size(330, 23);
            progressBarCreation.TabIndex = 9;
            // 
            // FormHome
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(354, 386);
            Controls.Add(progressBarCreation);
            Controls.Add(buttonHelp);
            Controls.Add(buttonReset);
            Controls.Add(buttonGo);
            Controls.Add(listViewContents);
            Controls.Add(textBoxReference);
            Controls.Add(comboBoxCI);
            MinimumSize = new Size(370, 425);
            Name = "FormHome";
            Text = "EvidenceMaster Beta v0.2.6";
            FormClosed += FormHome_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        private ComboBox comboBoxCI;
        private TextBox textBoxReference;
        private ListView listViewContents;
        private Button buttonGo;
        private Button buttonReset;
        private Button buttonHelp;
        private ProgressBar progressBarCreation;
    }
}