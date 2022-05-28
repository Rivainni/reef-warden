using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    void LoadData(PlayerState playerState);
    void SaveData(ref PlayerState playerState);
}
