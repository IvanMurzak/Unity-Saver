using UnityEngine;
using System.Security.Cryptography;
using System.IO;

public static class EncryptionUtils
{
//#if UNITY_EDITOR
//	private static readonly string KEY	= "unity_editor";
//#else
	private static readonly string KEY	= SystemInfo.deviceUniqueIdentifier + "_";
//#endif
	private static readonly byte[] SALT = new byte[] { 0x43, 0x87, 0x23, 0x72 };

	// Should be called from main thread!
	public static void Init() { }

	public static byte[] Encrypt(byte[] input)
	{
		var pdb = new PasswordDeriveBytes(KEY, SALT); 
		var ms	= new MemoryStream();
		var aes = new AesManaged();
		aes.Key = pdb.GetBytes(aes.KeySize / 8);
		aes.IV	= pdb.GetBytes(aes.BlockSize / 8);
		var cs	= new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);

		cs.Write	(input, 0, input.Length);
		cs.Close	();

		return ms.ToArray();
	}
	public static byte[] Decrypt(byte[] input)
	{
		var pdb	= new PasswordDeriveBytes(KEY, SALT);
		var ms	= new MemoryStream();
		var aes	= new AesManaged();
		aes.Key	= pdb.GetBytes(aes.KeySize / 8);
		aes.IV	= pdb.GetBytes(aes.BlockSize / 8);
		var cs	= new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);

		cs.Write	(input, 0, input.Length);
		cs.Close	();

		return ms.ToArray();
	}
}