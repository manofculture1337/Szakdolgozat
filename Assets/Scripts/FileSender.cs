using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public struct FileChunkMessage : NetworkMessage
{
    public int fileId;
    public int totalChunks;
    public int chunkIndex;
    public byte[] data;
}


public class FileSender : MonoBehaviour
{
    private const int _chunkSize = 16000;

    private const string _receivedFileName = "received_file.obj";

    private Dictionary<int, List<byte[]>> _files = new Dictionary<int, List<byte[]>>();


    public void SendFileToAll()
    {
        SendFileToServer(File.ReadAllBytes("C:/Users/szeke/Downloads/meta_room_mesh.obj"));
    }

    public void SendFileToTarget(NetworkConnectionToClient target)
    {
        byte[][] chunks = SplitFile(File.ReadAllBytes(_receivedFileName));
        int id = UnityEngine.Random.Range(0, int.MaxValue);

        for (int i = 0; i < chunks.Length; i++)
        {
            FileChunkMessage msg = new FileChunkMessage
            {
                fileId = id,
                totalChunks = chunks.Length,
                chunkIndex = i,
                data = chunks[i]
            };

            target.Send(msg);
        }
    }

    private void SendFileToServer(byte[] file)
    {
        byte[][] chunks = SplitFile(file);
        int id = UnityEngine.Random.Range(0, int.MaxValue);

        for (int i = 0; i < chunks.Length; i++)
        {
            FileChunkMessage msg = new FileChunkMessage
            {
                fileId = id,
                totalChunks = chunks.Length,
                chunkIndex = i,
                data = chunks[i]
            };

            NetworkClient.Send(msg);
        }
    }
    
    private byte[][] SplitFile(byte[] data)
    {
        int numOfChunks = Mathf.CeilToInt((float)data.Length / _chunkSize);
        byte[][] result = new byte[numOfChunks][];

        for (int i = 0; i < numOfChunks; i++)
        {
            int size = Mathf.Min(_chunkSize, data.Length - (i * _chunkSize));
            result[i] = new byte[size];
            Buffer.BlockCopy(data, i * _chunkSize, result[i], 0, size);
        }

        return result;
    }

    public void OnReceiveFileChunkFromClient(NetworkConnectionToClient sender, FileChunkMessage msg)
    {
        foreach (var conns in NetworkServer.connections)
        {
            if (conns.Value != sender)
            {
                conns.Value.Send(msg);
            }
        }

        OnReceiveChunk(msg);
        NetworkManager.singleton.gameObject.GetComponent<MyNetworkManager>().FileSent();
    }

    public void OnReceiveChunk(FileChunkMessage msg)
    {
        if (!_files.ContainsKey(msg.fileId))
        {
            _files[msg.fileId] = new List<byte[]>(msg.totalChunks);
        }

        List<byte[]> chunks = _files[msg.fileId];

        while (chunks.Count < msg.totalChunks)
        {
            chunks.Add(null);
        }

        chunks[msg.chunkIndex] = msg.data;

        if (chunks.All(c => c != null))
        {
            Debug.Log("File transfer complete! ID:" + msg.fileId);

            byte[] file = CombineChunks(chunks);
            File.Delete(_receivedFileName);
            File.WriteAllBytes(_receivedFileName, file);
            _files.Remove(msg.fileId);
        }
    }

    byte[] CombineChunks(List<byte[]> chunks)
    {
        int size = chunks.Sum(c => c.Length);
        byte[] result = new byte[size];

        int offset = 0;
        foreach (var chunk in chunks)
        {
            Buffer.BlockCopy(chunk, 0, result, offset, chunk.Length);
            offset += chunk.Length;
        }

        return result;
    }

}
