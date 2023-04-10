using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;

    public CharacterData_SO templateData;

    public CharacterData_SO characterData;

    public AttackData_SO attackData;

    [HideInInspector]

    public bool isCritical;

    void Awake()
    {
        if (templateData != null)
            characterData = Instantiate(templateData);    //这里是将template里存储的数据给到characterdata，这样复数的敌人使用的data就是分开的了
    }



    #region Read from Data_SO
    public int MaxHealth
    {
        get { if (characterData != null) return characterData.maxHealth; else return 0; }
        set { characterData.maxHealth = value; }
    }

    public int CurrentHealth
    {
        get { if (characterData != null) return characterData.currentHealth; else return 0; }
        set { characterData.currentHealth = value; }
    }

    public int BaseDefence
    {
        get { if (characterData != null) return characterData.baseDefence; else return 0; }
        set { characterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get { if (characterData != null) return characterData.currentDefence; else return 0; }
        set { characterData.currentDefence = value; }
    }
    #endregion


    #region Character Combat

    public void TakeDamage(CharacterStats attacker, CharacterStats defener)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence, 0);  //因为有可能出现，damage计算之后是负数的情况，这样就变成给角色回血了。因此要锁住最低不能低于0
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);



        if (attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }

        //update UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);



        //经验update

        if (CurrentHealth <= 0)
            attacker.characterData.UpdateExp(characterData.killPoint);

    }

    public void TakeDamage(int damage, CharacterStats defener)   //这个地方叫方法的重载
    {
        int currentDamage = Mathf.Max(damage - defener.CurrentDefence, 0);  //0作为兜底，即伤害太低的话就是0，反正不可能出现一个负数
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

    }





    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);

        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("暴击" + coreDamage);
        }
        return (int)coreDamage;
    }


    #endregion


}
