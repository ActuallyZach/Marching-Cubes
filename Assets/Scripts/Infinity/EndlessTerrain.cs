﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDist = 100;
    public Transform viewer;
    public Material material;

    public static Vector3 viewerPosition;
    static MapGenerator mapGenerator;

    int chunkSize = 16;
    int chunksVisibleInViewDst; // Chunks to render around the player

    Dictionary<Vector3, chunk> chunkDictionary = new Dictionary<Vector3, chunk>();
    List<chunk> chunksVisibleLastUpdate = new List<chunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector3(viewer.position.x, viewer.position.y, viewer.position.z);
        updateVisibleChunks();
    }

    void updateVisibleChunks()
    {
        for(int i = 0; i < chunksVisibleLastUpdate.Count; i ++)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        int currentChunkCoordZ = Mathf.RoundToInt(viewerPosition.z / chunkSize);

        for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
        {
            for (int zOffset = -chunksVisibleInViewDst; zOffset <= chunksVisibleInViewDst; zOffset++)
            {
                for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
                {
                    Vector3 viewedChunkCoord = new Vector3(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset, currentChunkCoordZ + zOffset);

                    if(chunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        chunkDictionary[viewedChunkCoord].UpdateChunk();
                        if(chunkDictionary[viewedChunkCoord].isVisible())
                        {
                            chunksVisibleLastUpdate.Add(chunkDictionary[viewedChunkCoord]);
                        }
                    } else
                    {
                        chunkDictionary.Add(viewedChunkCoord, new chunk(viewedChunkCoord, chunkSize, transform, material));
                    }
                }
            }
        }
    }

    public class chunk
    {
        GameObject meshObject;
        Vector3 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public chunk(Vector3 coord, int size, Transform parent, Material material)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector3.one * size);

            meshObject = new GameObject("Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = position;
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.requestMapData(onMapDataRecieved, position);
        }

        public void UpdateChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool isVisible = viewerDstFromNearestEdge <= maxViewDist;
            SetVisible(isVisible);
        }

        public void SetVisible(bool Visible)
        {
            meshObject.SetActive(Visible);
        }
        public bool isVisible()
        {
            return meshObject.activeSelf;
        }

        void onMapDataRecieved(MapData mapData)
        {
            mapGenerator.requestMeshData(mapData, onMeshDataReceived);
        }

        void onMeshDataReceived(VoxelMeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }
    }
}
