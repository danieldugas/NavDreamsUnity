using System.Collections.Generic;
using UnityEngine;

public class SemanticLabel : MonoBehaviour
{
    [Tooltip("The main label of the object ('wall', 'dog', etc..)")]
    public string label = "";
    // A more future-proof idea is to put the most low-level label here, and then
    // use some form of hierarchy based on parent transform (e.g. 'chair leg' but is child/part of 'chair', etc)
    // problem is defining common hierarchy levels.
}
