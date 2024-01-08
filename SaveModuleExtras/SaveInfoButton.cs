using Godot;
using System;
using System.Collections.Generic;

public class SaveInfoButton : Button
{
	public string name;
	public DateTime timeSaved;
	public string baseInfo;
	public int saveID;
	public string savePath;

	public static string richTextLabelName = "SaveNameLabel";
	public static string baseInfoLabelName = "BaseInfoLabel";
	public override void _Ready()
	{
		timeSaved = System.DateTime.Now;
	}


	public string DisplayString()
	{
		return $"{name}\n{timeSaved.ToShortDateString()}\n{baseInfo}";
	}

	public void UpdateBaseInfo(BuildableEditor buildableEditor)
	{
		int numBuildables = 0;
		foreach (Node node in buildableEditor.GetChildren())
		{
			if (node is Buildable buildable)
				numBuildables++;
		}
		baseInfo = $"number of pieces: {numBuildables}";
	}


	public void EnableTextEditing()
	{

		RichTextLabel richTextLabelSave = null;
		TextEdit textEditSave = null;
		foreach (Node node in GetChildren())
		{

			if (node is RichTextLabel richTextLabel)
			{
				if (richTextLabel.Name == richTextLabelName)
				{
					richTextLabelSave = richTextLabel;
				}
			}
			if (node is TextEdit textEdit)
			{

				textEditSave = textEdit;
			}

		}
		if (richTextLabelSave == null || textEditSave == null)
		{
			return;
		}
		textEditSave.Text = richTextLabelSave.Text;
		textEditSave.Visible = true;
		textEditSave.SetBlockSignals(true);

		// richTextLabelSave.Text = "";
		richTextLabelSave.Visible = false;
		richTextLabelSave.SetBlockSignals(false);

		name = textEditSave.Text;

	}

	public void DisableTextEditing()
	{
		RichTextLabel richTextLabelSave = null;
		TextEdit textEditSave = null;
		foreach (Node node in GetChildren())
		{

			if (node is RichTextLabel richTextLabel)
			{
				if (richTextLabel.Name == richTextLabelName)
				{
					richTextLabelSave = richTextLabel;
				}
			}
			if (node is TextEdit textEdit)
			{
				textEditSave = textEdit;
			}

		}
		if (richTextLabelSave == null || textEditSave == null)
		{
			return;
		}
		richTextLabelSave.Text = textEditSave.Text;
		richTextLabelSave.Visible = true;
		richTextLabelSave.SetBlockSignals(true);

		// textEditSave.Text = "";
		textEditSave.Visible = false;
		textEditSave.SetBlockSignals(false);

		name = richTextLabelSave.Text;
	}

	public string UpdateBaseInfo()
	{
		timeSaved = System.DateTime.Now;
		string baseInfoStr = $"saveID:{saveID} saved in {savePath}\n time saved: {timeSaved}";
		foreach (Node node in GetChildren())
		{
			if (node is RichTextLabel richTextLabel)
			{
				if (richTextLabel.Name == baseInfoLabelName)
				{
					richTextLabel.Text = baseInfoStr;
					return baseInfoStr;
				}
			}
		}
		return baseInfoStr;
	}

	public void SetTextToDefaultText()
	{
		string saveFolderPath = System.IO.Directory.GetCurrentDirectory();
		string[] files =
			System.IO.Directory.GetFiles(saveFolderPath);
		int saveNum = 1;
		foreach (string filepath in files)
		{
			int savePosition = filepath.IndexOf("Save");
			if (savePosition >= 0 &&
				filepath.BaseName().Substring(savePosition, savePosition + 5) == "Save")
			{
				if (Int32.TryParse(filepath.BaseName().Substring(savePosition + 5), out saveNum))
				{
					saveNum++;
				}
			}
		}
		string defaultText = $"Save{saveNum}";
		UpdateBaseInfo();
		foreach (Node node in GetChildren())
		{
			if (node is RichTextLabel richTextLabel)
			{
				if (richTextLabel.Name == richTextLabelName)
				{
					richTextLabel.Text = defaultText;
				}
			}
			if (node is TextEdit textEdit)
			{
				textEdit.Text = defaultText;
			}
		}
		DisableTextEditing();
	}
}
