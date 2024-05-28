using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities 
{
    public static T[] ShuffleArray<T>(T[] _dataArray, int _seed)
    {
        System.Random prng = new System.Random(_seed);

        for(int i = 0; i < _dataArray.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, _dataArray.Length);

            T temp = _dataArray[randomIndex];
            _dataArray[randomIndex] = _dataArray[i];
            _dataArray[i] = temp;
        }

        return _dataArray;
    }
}
