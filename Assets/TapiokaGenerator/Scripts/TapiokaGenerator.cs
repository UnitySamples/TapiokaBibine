using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// タピオカがバイバイン
/// </summary>
public class TapiokaGenerator : MonoBehaviour
{
    #region UI Connection
    [SerializeField] TMP_Text textGUI = null;
    #endregion

    #region Assets
    [SerializeField] GameObject prefab = null;
    #endregion

    void Start()
    {
        // PrefabEntityをPrefabObjectから作成
        this.prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        // 最初の一個を原点に作成（ディフォルト値がゼロなので）
        this.entities = new NativeArray<Entity>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        World.Active.EntityManager.Instantiate(this.prefabEntity, this.entities);

        this.textGUI.text = $"{this.entities.Length.ToString("0")} [Tapioka]";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DoubleEntities();

            this.textGUI.text = $"{this.entities.Length.ToString("0")} [Tapioka]";
        }
    }

    /// <summary>
    /// 現在のエンティティを二倍にする
    /// </summary>
    unsafe void DoubleEntities()
    {
        // 2倍のエンティティ配列を確保して、前半分に既存のエンティティ情報をメモリコピー
        var doubleEntities = new NativeArray<Entity>(this.entities.Length * 2,
            Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        UnsafeUtility.MemCpy(doubleEntities.GetUnsafePtr(),
                this.entities.GetUnsafePtr(), UnsafeUtility.SizeOf(typeof(Entity)) * this.entities.Length);
        // 後ろ半分のエンティティ配列をオフセットを与えてノー確保で別名として作成
        var newEntities = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Entity>(
            (Entity*)doubleEntities.GetUnsafePtr() + entities.Length,
            entities.Length, Allocator.None);
        // Editorで実行する際に安全に読み書きを行うためのおまじない
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref newEntities,
            NativeArrayUnsafeUtility.GetAtomicSafetyHandle(doubleEntities));
#endif
        // 後ろ半分のエンティティ配列にエンティティを作成して詰める
        var manager = World.Active.EntityManager;
        manager.Instantiate(this.prefabEntity, newEntities);
        // エンティティの初期位置を前半分のエンティティと同じにする
        var entityIndex = 0;
        foreach (var entity in this.entities)
        {
            manager.SetComponentData(newEntities[entityIndex++],
                manager.GetComponentData<Translation>(entity));
        }
        // 新しいエンティティ配列を保持
        this.entities.Dispose();
        this.entities = doubleEntities;
    }

    void OnDestroy()
    {
        // 後処理
        this.entities.Dispose();
    }

    // 現在のエンティティ
    NativeArray<Entity> entities = new NativeArray<Entity>();
    // PrefabObjectから作ったPrefabEntity
    Entity prefabEntity;
}
