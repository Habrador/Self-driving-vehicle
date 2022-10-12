using UnityEngine;
using System.Collections;
using System;

namespace PathfindingForVehicles
{
    //Heap from https://www.youtube.com/watch?v=3Dw5d7PlcTM
    //T in this case will be Node
    //T : IHeapItem<T> means that the Node has to implement the interface
    public class Heap<T> where T : IHeapItem<T>
    {
        //The array that will hold the heap
        private T[] items;
        //How many nodes we have stored in the heap
        private int currentItemCount;



        //How many items can we have in the heap?
        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }



        //Add new item to the heap
        public void Add(T item)
        {
            //Do we have room to add it?
            if (currentItemCount + 1 > items.Length)
            {
                //Debug.Log("Cant add item to heap becuse it's full");

                return;
            }
        
            item.HeapIndex = currentItemCount;

            //Add the item to the end of the array
            items[currentItemCount] = item;

            //But it may belong to another position in the heap
            SortUp(item);

            currentItemCount += 1;
        }



        //Remove the first item from the heap, which is the node with the lowest f cost
        public T RemoveFirst()
        {
            T firstItem = items[0];

            currentItemCount -= 1;

            //To resort the heap, we add the last item in the array to the first position in the array
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;

            //And then move the first item to where it belongs in the array
            SortDown(items[0]);

            return firstItem;
        }



        //How many items do we have in the heap?
        public int Count
        {
            get
            {
                return currentItemCount;
            }
        }



        //Does the heap contain this item?
        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }



        //Update an item already in the heap, but we need to change its priority in the heap
        public void UpdateItem(T item)
        {
            //This is for pathfinding so we only need to add better nodes and thus only need to sort up
            SortUp(item);
        }



        //Clear the array
        public void Clear()
        {
            Array.Clear(items, 0, items.Length);

            currentItemCount = 0;
        }



        //
        // Heap mechanics
        //

        //Sorts and item down in the array to the position where it belongs
        private void SortDown(T item)
        {
            while (true)
            {
                //From heap index to array index
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;

                int swapIndex = 0;

                //Do we have a children to the left
                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;

                    //But we also need to check if we have a children to the right
                    if (childIndexRight < currentItemCount)
                    {
                        //Compare the left and the right node, to find if we should swap with the left or the right node
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }



        //Sorts an item up in the array to the position where it belongs
        private void SortUp(T item)
        {
            //From heap index to array index
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = items[parentIndex];

                //If item has a lower f cost than the parent
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }



        //Swap 2 items in the heap, which is the same as moving one item up (or down) and the other item down (or up)
        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;

            //We also need to swap the heap indexes
            int itemAIndex = itemA.HeapIndex;

            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
    }



    //Each node has to implement this, so both HeapIndex and CompareTo
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex
        {
            get;
            set;
        }
    }
}
