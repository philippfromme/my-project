using UnityEngine;

public enum DriveType
{
    RWD,
    FWD,
    AWD 
}


[CreateAssetMenu(fileName = "NewCarData", menuName = "Cars/CarData")]
public class CarData : ScriptableObject
{
    public CarView carView;

    public string carName;
    public GameObject carPrefab;

    [Range(50, 200)]
    public int maxSpeed = 100;

    [Range(50, 1000)]
    public int power = 100;

    [Range(500, 1500)]
    public int weight = 1000;

    [SerializeField] private DriveType driveType; // Dropdown for drive type

    public DriveType GetDriveType()
    {
        return driveType;
    }

    public void SetDriveType(DriveType newDriveType)
    {
        driveType = newDriveType;
    }
}
