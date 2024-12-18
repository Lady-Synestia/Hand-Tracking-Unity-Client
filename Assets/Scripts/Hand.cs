using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] Transform[] childTransforms;

    // Multiplier for raw position data
    // stops points from being so clustered
    // TODO: fix offset caused by multiplier
    const int distanceMultiplier = -1;

    private void Start()
    {
        // number of children is expected to be 21
        // storing transforms of children so they don't have to be found again later
        //childTransforms = new Transform[this.transform.childCount];
        //for (int i = 0; i < this.transform.childCount; i++)
        //{
        //    childTransforms[i] = this.transform.GetChild(i);
        //}
    }

    public void SetChildPosition(int childIndex, Vector3 position)
    {
        childTransforms[childIndex].position = position * distanceMultiplier;
    }
}
