using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Formation : IComponentData
{
    public float3 Position;
    public Entity Squad;
    public Entity Unit;
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class FormationSystem : SystemBase
{
    private double lastUpdateTime;
        
    protected override void OnUpdate()
    {
        var localComp = GetComponentDataFromEntity<LocalToWorld>(true);
        Entities
            .WithBurst()
            .WithReadOnly(localComp)
            .WithAll<Formation>()
                .ForEach((ref Translation translation, in Formation formation) => 
                {
                    translation.Value = FormationUtils.LocalToWorld(localComp[formation.Squad].Value, formation.Position);
                }
            ).ScheduleParallel();
    }
}

public static class FormationUtils
{
    //get the relative location of the slot for a unit in the formation
    public static float3 LocalToWorld(float4x4 local, float3 slotPos)
    {
        var anchor = new float3(local.c3.x, local.c3.y, local.c3.z);
        var relPos = anchor;
        relPos += math.rotate(local, slotPos); //localize to the squad leader object

        // Convert to radians
        var radians = math.radians(new quaternion(local).ToEuler().y);
        var sin = math.sin(radians);
        var cos = math.cos(radians);
        
        //rotate around the squad controller object
        var rotatedX = cos * (relPos.x - anchor.x) - sin * (relPos.z - anchor.z) + anchor.x;
        var rotatedZ = sin * (relPos.x - anchor.x) + cos * (relPos.z - anchor.z) + anchor.z;
        relPos.x = rotatedX;
        relPos.z = rotatedZ;

        return relPos;
    }
    
    public static void GetPositions(List<Vector3> buffer, Vector3 center, float angle) 
    {
        var radians = math.radians(-angle);
        var cos = math.cos(radians);
        var sin = math.sin(radians);

        var terrain = Manager.terrain;
        
        for (var i = 0; i < buffer.Count; i++) {
            var pos = buffer[i];
            pos = center + new Vector3(pos.x * cos - pos.z * sin,0f, pos.x * sin + pos.z * cos);
            pos.y = terrain.SampleHeight(pos) + 0.5f;
            buffer[i] = pos;
        }
    }

    public static float SetFormation(ref EntityManager entityManager, List<Unit> units, List<Vector3> positions, float4x4 local, FormationShape formationShape, UnitSize size, int count, float length)
    {
        var shift = GetFormation(positions, formationShape, size, count, length);

        var terrain = Manager.terrain;
        
        var ordered = units.ToList();
        foreach (var position in positions) {
            Vector3 pos = LocalToWorld(local, position);
            pos.y = terrain.SampleHeight(pos);
            var entity = Entity.Null;
            foreach (var unit in ordered.OrderBy(u => Vector.DistanceSq(u.worldTransform.position, pos))) {
                entity = unit.formation;
                ordered.Remove(unit);
                break;
            }
            var formation = entityManager.GetComponentData<Formation>(entity);
            formation.Position = position;
            entityManager.SetComponentData(entity, formation);
        }

        return shift;
    }

    public static float GetFormation(List<Vector3> buffer, FormationShape formationShape, UnitSize size, int count, float length, bool inverse = false)
    {
        buffer.Clear();
        switch (formationShape) {
            case FormationShape.Phalanx:
                return PhalanxFormation(buffer, size, count, length, inverse);
            case FormationShape.Vee:
                return VeeFormation(buffer, size, count, length, inverse);
            default:
                throw new Exception("Invalid shape type");
        }
    }

    private static float VeeFormation(List<Vector3> buffer, UnitSize size, int count, float length, bool inverse)
    {
        var rows = (int) (length / size.width);
        if (rows <= 0) return 0f;
        
        var sx = -(rows * size.width);
        var sy = (rows * size.height) / 2f;
        var shift = inverse ? 0f : sy;
        var cx = size.width / 2f;
        var cy = size.height / 2f;
        
        var height = rows;
        for (var row = 1; row <= rows; ++row) {
            int max; // max for current row
            if (row > count) {
                max = count;
                height += max / row;
            } else {
                max = row;
            }
            for (var col = 1; col <= max; col++) {
                buffer.Add(new Vector3(sx + (height + max - 2 * col) * size.width + cx, 0f, shift - (row + cy)));
                count--;
                if (count == 0) {
                    return sy;
                }
            }
        }

        return 0f;
    }
    
    private static float PhalanxFormation(List<Vector3> buffer, UnitSize size, int count, float length, bool inverse) 
    {
        var cols = (int) (length / size.width);
        if (cols <= 0) return 0f;
        var rows = count / cols;
        var orphans = count % cols;
        
        var sx = -(cols * size.width) / 2f;
        var sy =  (orphans > 0 ? rows + 1 : rows) * size.height;
        var shift = inverse ? 0f : sy;
        var cx = size.width / 2f;
        var cy = size.height / 2f;
        
        var height = 0f;
        for (var row = 0; row < rows; ++row) {
            for (var col = 0; col < cols; ++col) {
                buffer.Add(new Vector3(sx + col * size.width + cx, 0f, shift - (height + cy)));
            }
            height += size.height;
        }

        sx += (cols - orphans) * cx;
        for (var col = 0; col < orphans; ++col) {
            buffer.Add(new Vector3(sx + col * size.width + cx, 0f, shift - (height + cy)));
        }

        return sy;
    }
}

public enum FormationShape
{
    Phalanx,
    Vee
}