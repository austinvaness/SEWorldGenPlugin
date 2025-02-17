﻿using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGeneration
{
    /// <summary>
    /// Cell based generator module. Will only generate objects of loaded cells
    /// and will handle unloading of said cells.
    /// </summary>
    public abstract class MyAbstractProceduralCellModule : IMyProceduralGeneratorModule
    {
        /// <summary>
        /// Seed used in this module
        /// </summary>
        protected int m_seed;

        /// <summary>
        /// All currently loaded cells
        /// </summary>
        protected Dictionary<Vector3I, MyProceduralCell> m_loadedCells = new Dictionary<Vector3I, MyProceduralCell>();

        /// <summary>
        /// Tree of cell positions, to get cells based on bounding boxes, spheres etc...
        /// </summary>
        protected MyDynamicAABBTreeD m_cellsTree = new MyDynamicAABBTreeD(Vector3D.Zero);

        /// <summary>
        /// All cells, that should get unloaded, if no entity is in range of it.
        /// </summary>
        private CachingHashSet<MyProceduralCell> m_toUnloadCells = new CachingHashSet<MyProceduralCell>();

        /// <summary>
        /// All cells, that should get loaded, if entities are in range of it.
        /// </summary>
        private CachingHashSet<Vector3I> m_toLoadCells = new CachingHashSet<Vector3I>();

        /// <summary>
        /// All currently existing object seeds.
        /// </summary>
        protected HashSet<MyObjectSeed> m_existingObjectSeeds = new HashSet<MyObjectSeed>();

        /// <summary>
        /// Size of the cells of this module
        /// </summary>
        protected double m_cellSize;

        /// <summary>
        /// Creates a new cell module with given seed and cell size.
        /// If the Cellsize is larger than 25000, it wont be used and 25000 will be used instead
        /// </summary>
        /// <param name="seed">Seed of the module</param>
        /// <param name="cellSize">Size of the indevidual cells</param>
        public MyAbstractProceduralCellModule(int seed, double cellSize)
        {
            m_seed = seed;
            m_cellSize = Math.Min(cellSize, 25000);
        }

        /// <summary>
        /// Generates a new Procedural cell and generates its seeds inside it.
        /// Does not generate the objects of the seeds itself
        /// </summary>
        /// <param name="cellId">The cell id of the cell</param>
        /// <returns>The new Procedural cell</returns>
        protected abstract MyProceduralCell GenerateCellSeeds(Vector3I cellId);

        /// <summary>
        /// Method to call when simulation updates, to update all cells if necessary
        /// </summary>
        public abstract void UpdateCells();

        /// <summary>
        /// Generates all objects inside the currently loaded cells, if they are not
        /// already generated.
        /// </summary>
        public abstract void GenerateLoadedCellObjects();

        /// <summary>
        /// Closes the specified object. This should remove it from the world and memory.
        /// </summary>
        /// <param name="seed">The MyObjectSeed of the object that should be removed.</param>
        public abstract void CloseObject(MyObjectSeed seed);

        public abstract void UpdateGpsForPlayer(MyEntityTracker entity);

        /// <summary>
        /// Marks cells to load or keep loaded inside the bounds
        /// </summary>
        /// <param name="bounds">Spherical bounds</param>
        public void MarkToLoadCellsInBounds(BoundingSphereD bounds)
        {
            BoundingBoxD box = BoundingBoxD.CreateFromSphere(bounds);
            Vector3I cellId = Vector3I.Floor(box.Min / m_cellSize);

            for (var it = GetCellsIterator(box); it.IsValid(); it.GetNext(out cellId))
            {
                if (m_toLoadCells.Contains(cellId)) continue;

                BoundingBoxD cellBounds = new BoundingBoxD(cellId * m_cellSize, (cellId + 1) * m_cellSize);
                if (bounds.Contains(cellBounds) == ContainmentType.Disjoint) continue;

                m_toLoadCells.Add(cellId);
            }

            m_toLoadCells.ApplyAdditions();
        }

        /// <summary>
        /// Gets all seeds within said bounds.
        /// If a cell inside the bounds is not loaded, it will generate the seed but wont load the cell.
        /// </summary>
        /// <param name="bounds">Bounds to get seeds from</param>
        /// <param name="list">List to put seeds into.</param>
        public void GetSeedsInBounds(BoundingSphereD bounds, List<MyObjectSeed> list)
        {
            BoundingBoxD box = BoundingBoxD.CreateFromSphere(bounds);
            Vector3I cellId = Vector3I.Floor(box.Min / m_cellSize);
            for (var it = GetCellsIterator(box); it.IsValid(); it.GetNext(out cellId))
            {
                if (m_loadedCells.ContainsKey(cellId))
                {
                    m_loadedCells[cellId].GetAll(list, false);
                }
                else
                {
                    MyProceduralCell cell = GenerateCellSeeds(cellId);
                    cell.GetAll(list);
                }
            }
        }

        /// <summary>
        /// Gets an iterator for all Vector3I within the bounding box bbox
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Vector3I Iterator for all vectors inside bbox</returns>
        protected Vector3I_RangeIterator GetCellsIterator(BoundingBoxD bbox)
        {
            Vector3I min = Vector3I.Floor(bbox.Min / m_cellSize);
            Vector3I max = Vector3I.Floor(bbox.Max / m_cellSize);

            return new Vector3I_RangeIterator(ref min, ref max);
        }

        /// <summary>
        /// Generates all marked to be loaded cells seeds
        /// </summary>
        public void LoadCells()
        {
            m_toLoadCells.ApplyAdditions();

            foreach(var cellId in m_toLoadCells)
            {
                if (m_loadedCells.ContainsKey(cellId)) continue;

                MyProceduralCell cell = GenerateCellSeeds(cellId);
                if (cell != null)
                {
                    m_loadedCells.Add(cellId, cell);

                    BoundingBoxD aabb = cell.BoundingVolume;
                    cell.proxyId = m_cellsTree.AddProxy(ref aabb, cell, 0u);
                }
            }

            m_toLoadCells.Clear();
        }

        /// <summary>
        /// Unloads all marked cells, except those, that are also marked to be loaded, due 
        /// to overlapping bounds when marking.
        /// </summary>
        public void UnloadCells()
        {
            List<Vector3I> unloadCells = new List<Vector3I>();
            foreach(var cell in m_loadedCells)
            {
                if (m_toLoadCells.Contains(cell.Key)) continue;
                unloadCells.Add(cell.Key);
            }

            foreach(var cellid in unloadCells)
            {
                var cell = m_loadedCells[cellid];

                List<MyObjectSeed> seeds = new List<MyObjectSeed>();

                cell.GetAll(seeds);

                foreach (var seed in seeds)
                {
                    if (seed.Params.Generated)
                    {
                        CloseObject(seed);
                    }
                }
                seeds.Clear();

                m_loadedCells.Remove(cellid);
                m_cellsTree.RemoveProxy(cell.proxyId);
            }

            return;
        }

        /// <summary>
        /// Gets the seed of the object based on the objects hashcode.
        /// </summary>
        /// <param name="obj">The MyObjectSeed for the object</param>
        /// <returns>The seed of the object</returns>
        protected int GetObjectIdSeed(MyObjectSeed obj)
        {
            int hash = obj.CellId.GetHashCode();
            hash = (hash * 397) ^ m_seed;
            hash = (hash * 397) ^ obj.Params.Index;
            hash = (hash * 397) ^ obj.Params.Seed;
            return hash;
        }
    }
}
