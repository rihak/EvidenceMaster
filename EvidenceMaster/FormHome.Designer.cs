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
            linkLabelHelp = new LinkLabel();
            SuspendLayout();
            // 
            // comboBoxCI
            // 
            comboBoxCI.AutoCompleteMode = AutoCompleteMode.Suggest;
            comboBoxCI.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxCI.FormattingEnabled = true;
            comboBoxCI.Location = new Point(12, 12);
            comboBoxCI.Name = "comboBoxCI";
            comboBoxCI.Size = new Size(328, 23);
            comboBoxCI.TabIndex = 2;
            // 
            // textBoxReference
            // 
            textBoxReference.Location = new Point(12, 41);
            textBoxReference.Name = "textBoxReference";
            textBoxReference.PlaceholderText = "Reference";
            textBoxReference.Size = new Size(328, 23);
            textBoxReference.TabIndex = 4;
            // 
            // listViewContents
            // 
            listViewContents.AllowDrop = true;
            listViewContents.Location = new Point(12, 70);
            listViewContents.Name = "listViewContents";
            listViewContents.Size = new Size(328, 275);
            listViewContents.TabIndex = 5;
            listViewContents.UseCompatibleStateImageBehavior = false;
            listViewContents.View = View.List;
            listViewContents.DragDrop += listViewContents_DragDrop;
            listViewContents.DragEnter += listViewContents_DragEnter;
            listViewContents.KeyDown += listViewContents_KeyDown;
            // 
            // buttonGo
            // 
            buttonGo.BackColor = SystemColors.ActiveCaption;
            buttonGo.Location = new Point(265, 351);
            buttonGo.Name = "buttonGo";
            buttonGo.Size = new Size(75, 23);
            buttonGo.TabIndex = 6;
            buttonGo.Text = "Go";
            buttonGo.UseVisualStyleBackColor = false;
            buttonGo.Click += buttonGo_Click;
            // 
            // buttonReset
            // 
            buttonReset.Location = new Point(184, 351);
            buttonReset.Name = "buttonReset";
            buttonReset.Size = new Size(75, 23);
            buttonReset.TabIndex = 7;
            buttonReset.Text = "Reset";
            buttonReset.UseVisualStyleBackColor = true;
            buttonReset.Click += buttonReset_Click;
            // 
            // linkLabelHelp
            // 
            linkLabelHelp.AutoSize = true;
            linkLabelHelp.Location = new Point(12, 355);
            linkLabelHelp.Name = "linkLabelHelp";
            linkLabelHelp.Size = new Size(12, 15);
            linkLabelHelp.TabIndex = 8;
            linkLabelHelp.TabStop = true;
            linkLabelHelp.Text = "?";
            linkLabelHelp.LinkClicked += linkLabelHelp_LinkClicked;
            // 
            // FormHome
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(353, 386);
            Controls.Add(linkLabelHelp);
            Controls.Add(buttonReset);
            Controls.Add(buttonGo);
            Controls.Add(listViewContents);
            Controls.Add(textBoxReference);
            Controls.Add(comboBoxCI);
            Name = "FormHome";
            Text = "EvidenceMaster";
            ResumeLayout(false);
            PerformLayout();
        }

        private ComboBox comboBoxCI;
        private TextBox textBoxReference;
        private ListView listViewContents;
        private Button buttonGo;
        private Button buttonReset;
        private LinkLabel linkLabelHelp;
    }
}