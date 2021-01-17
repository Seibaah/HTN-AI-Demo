using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class monster_htn
{
    public compound_task rootNode;

    public GameObject monster;
    public monster_controller m;
    
    //default constructor builds the htn tree model in the implementation doc
    public monster_htn()
    {
        monster = GameObject.Find("Monster");
        m = monster.GetComponent<monster_controller>();

        //define rootNode node
        rootNode = new compound_task(node.type.Root, "beAMonster");
        rootNode.children = new List<node>();

        rootNode.children.Add(new compound_task(rootNode, node.type.Compound, compound_task.subType.Sequence, "wander"));
        rootNode.children.Add(new compound_task(rootNode, node.type.Compound, compound_task.subType.Sequence, "attack"));

        //build left handside of the htn: wandering behavior
        compound_task wanderNode = (compound_task)rootNode.children[0];
        wanderNode.children = new List<node>
        {
            new compound_task(wanderNode, node.type.Compound,compound_task.subType.Selector, "navSpeed"),   //movement type
            new primitive_task(wanderNode, node.type.Primitive, "setNavDest"),  //choose dest
            new primitive_task(wanderNode, node.type.Primitive, "idle")  //rest
        };

        compound_task navSpeedNode = (compound_task)wanderNode.children[0];
        navSpeedNode.children = new List<node>
        {
            new primitive_task(navSpeedNode, node.type.Primitive, "walk"),
            new primitive_task(navSpeedNode, node.type.Primitive, "run")
        };

        //build the right handside of the tree: attack behavior
        compound_task attackNode = (compound_task)rootNode.children[1];
        attackNode.children = new List<node>
        {
            new primitive_task(attackNode, node.type.Primitive, "getAngry"), //change skin color
            new compound_task(attackNode, node.type.Compound, compound_task.subType.Sequence, "findObjectToThrow"),    //pick up object
            new compound_task(attackNode, node.type.Compound, compound_task.subType.Selector, "throw"),   //throw
            new primitive_task(wanderNode, node.type.Primitive, "recover")  //rest
        };

        compound_task findObjNode = (compound_task)attackNode.children[1];
        findObjNode.children = new List<node>
        {
            new primitive_task (findObjNode, node.type.Primitive, "findObj"),   //find closest obj
            new primitive_task(findObjNode, node.type.Primitive, "navToObj"),   //navigate to it
            new primitive_task(findObjNode, node.type.Primitive, "pickUp")  //pick it up
        };

        compound_task throwNode = (compound_task)attackNode.children[2];
        throwNode.children = new List<node>
        {
            new compound_task(throwNode, node.type.Compound,compound_task.subType.Sequence, "superThrow"), //throw rock
            new primitive_task(throwNode, node.type.Primitive, "objectThrow")   //throw object
        };

        compound_task superThrowNode = (compound_task)throwNode.children[0];
        superThrowNode.children = new List<node>
        {
            new primitive_task(superThrowNode, node.type.Primitive, "growl"),   //growl
            new primitive_task(superThrowNode, node.type.Primitive, "objectThrow")  //throw object
        };
    }
}

//base class for each node of the tree
public class node
{
    public node parent;
    public type t;

    public enum type
    {
        Root,
        Compound,
        Primitive
    }
}

//1st type of specialized node, a compound task node holds children tasks that form its behavior
public class compound_task : node
{
    public List<node> children;
    public string behaviorName;
    public subType st;

    public enum subType
    {
        Selector,
        Sequence
    };

    public compound_task() { }

    public compound_task(type tp, string n)
    {
        t = tp;
        behaviorName = n;
    }

    public compound_task(node p, type tp, subType stp, string n)
    {
        parent = p;
        t = tp;
        behaviorName = n;
        st = stp;
    }
}

//2nd type of specialized node, a primitive task node is a task itself
public class primitive_task : node
{
    public string taskName;

    public primitive_task(node p, type tp, string n)
    {
        parent = p;
        t = tp;
        taskName = n;
    }
}