using Extensions.Saver;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonoBehaviourSaver : SaverMonoBehaviour<TestData>
{
	protected override string SaverPath		=> "TestDatabase";
	protected override string SaverFileName => "test.dat";
}

[Serializable]
public class TestData
{
	public string testString = "abc";
}
