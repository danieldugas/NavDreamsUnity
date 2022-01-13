using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IterTools
{
    public static List<int> ShuffledRange(int max) {
        List<int> list = new List<int>();
        for (int i = 0; i < max; i++) {
            list.Add(i);
        }
        for (int i = list.Count; i > 1; i -= 1)
        {
            int j = Random.Range(0, i); 
            int temp = list[j];
            list[j] = list[i - 1];
            list[i - 1] = temp;
        }
        return list;
    }
}