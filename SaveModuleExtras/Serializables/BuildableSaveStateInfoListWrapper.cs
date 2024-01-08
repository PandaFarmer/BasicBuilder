using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

public class BuildableSaveStateInfoListWrapper
{
	public string name;
	public DateTime timeSaved;
	public string baseInfo;
    public int saveID;
	public List<BuildableSaveStateInfo> buildableSaveStateInfos;
}