using System;
using Mirror;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.CleanArchitecture.Entities
{
    public struct FileChunkMessage : NetworkMessage
    {
        public int fileId;
        public int totalChunks;
        public int chunkIndex;
        public byte[] data;
    }


    internal class FileSender
    {
        private const int _chunkSize = 16000;

        private RoomMeshExporter _meshExporter;

        private bool sendAfterExport;

        public FileSender(RoomMeshExporter meshExporter, bool sendAfterExport = false)
        {
            _meshExporter = meshExporter;
            if(sendAfterExport)
                _meshExporter.OnExportCompleted.AddListener(SendMesh);
            this.sendAfterExport = sendAfterExport;
        }

        public void SendMesh()
        {
           SendFileToAll(_meshExporter.GetExportedFilePath());
        }

        public void SendFileToAll(string filePath)
        {
            SendFileToServer(File.ReadAllBytes(filePath));
        }

        private void SendFileToServer(byte[] file)
        {
            Debug.Log($"Sending {file.Length} byte of Mesh data");
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
                Debug.Log("And now sending");
                NetworkClient.Send(msg);
                Debug.Log("Mesh sent successfully");
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

    }
}
