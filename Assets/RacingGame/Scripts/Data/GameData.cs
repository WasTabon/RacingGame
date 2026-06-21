using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DriverData
{
    public string id;
    public string firstName;
    public string lastName;
    public int age;
    public string nationality;

    public int speed;
    public int qualifying;
    public int consistency;
    public int tireManagement;
    public int feedback;
    public int aggression;
    public int wetSkill;

    public int potential;

    public int salary;
    public int contractYears;
    public string teamId;
    public bool isAcademy;

    public int seasonPoints;

    public string FullName { get { return firstName + " " + lastName; } }

    public int OverallSkill
    {
        get
        {
            int sum = speed + qualifying + consistency + tireManagement + feedback + aggression + wetSkill;
            return Mathf.RoundToInt(sum / 7f);
        }
    }
}

[Serializable]
public class EngineerData
{
    public string id;
    public string firstName;
    public string lastName;
    public int age;

    public int skill;
    public int creativity;
    public int experience;
    public Specialization specialization;

    public int salary;
    public int contractYears;
    public string teamId;
    public DepartmentType assignedDepartment;
    public bool isAssigned;

    public string FullName { get { return firstName + " " + lastName; } }
}

[Serializable]
public class StaffData
{
    public string id;
    public string firstName;
    public string lastName;
    public StaffRole role;

    public int skill;
    public int experience;
    public int salary;
    public int contractYears;
    public string teamId;

    public string FullName { get { return firstName + " " + lastName; } }
}

[Serializable]
public class DepartmentData
{
    public DepartmentType type;
    public int level;
    public List<string> assignedEngineerIds = new List<string>();
    public string activeProjectId;
    public float outputRating;
}

[Serializable]
public class TechNode
{
    public string id;
    public string nodeName;
    public int level;
    public bool unlocked;
    public int unlockCost;
    public float performanceBonus;
}

[Serializable]
public class TechTree
{
    public TechTreeType type;
    public List<TechNode> nodes = new List<TechNode>();

    public int CurrentLevel
    {
        get
        {
            int lvl = 0;
            foreach (TechNode n in nodes)
            {
                if (n.unlocked && n.level > lvl) lvl = n.level;
            }
            return lvl;
        }
    }
}

[Serializable]
public class ProjectData
{
    public string id;
    public string projectName;
    public DepartmentType department;

    public int cost;
    public int totalWeeks;
    public int weeksRemaining;
    public float riskPercent;
    public float expectedGain;

    public ProjectStage stage;
    public bool isActive;
    public bool isComplete;
    public bool failed;
}

[Serializable]
public class CarData
{
    public string id;
    public string carName;

    public float frontWing;
    public float rearWing;
    public float floor;

    public float enginePower;
    public float engineEfficiency;

    public float weight;
    public float balance;
    public float suspension;

    public float reliability;

    public float downforceSetup;
    public float balanceSetup;
    public float reliabilitySetup;

    public float AeroRating { get { return (frontWing + rearWing + floor) / 3f; } }
    public float EngineRating { get { return (enginePower + engineEfficiency) / 2f; } }
    public float ChassisRating { get { return (balance + suspension + (100f - weight)) / 3f; } }
    public float OverallPerformance { get { return AeroRating * 0.4f + EngineRating * 0.35f + ChassisRating * 0.25f; } }
}

[Serializable]
public class FacilityData
{
    public FacilityType type;
    public int level;
    public int upgradeCost;
    public int upgradeWeeksRemaining;
    public bool isUpgrading;
}

[Serializable]
public class SponsorData
{
    public string id;
    public string sponsorName;
    public int perRacePayout;
    public int signingBonus;
    public int reputationRequired;
}

[Serializable]
public class TeamData
{
    public string id;
    public string teamName;
    public bool isPlayer;
    public TeamPhilosophy philosophy;

    public long money;
    public int reputation;
    public int researchPoints;
    public List<SponsorData> sponsors = new List<SponsorData>();

    public List<CarData> cars = new List<CarData>();
    public List<string> driverIds = new List<string>();
    public string[] raceDriverIds = new string[2];

    public List<string> engineerIds = new List<string>();
    public string teamPrincipalId;
    public string technicalDirectorId;

    public List<DepartmentData> departments = new List<DepartmentData>();
    public List<TechTree> techTrees = new List<TechTree>();
    public List<ProjectData> projects = new List<ProjectData>();
    public List<FacilityData> facilities = new List<FacilityData>();

    public int championshipPoints;
    public int seasonWins;

    public float aiCarPerformance;
}

[Serializable]
public class RaceResultEntry
{
    public string driverId;
    public string teamId;
    public int position;
    public int points;
    public float gapSeconds;
    public bool dnf;
}

[Serializable]
public class RaceData
{
    public string id;
    public string trackName;
    public string country;
    public int round;
    public int laps;
    public float trackLengthKm;
    public float weatherWetChance;

    public bool completed;
    public List<RaceResultEntry> results = new List<RaceResultEntry>();
}

[Serializable]
public class StandingEntry
{
    public string id;
    public int points;
    public int wins;
}

[Serializable]
public class SeasonData
{
    public int year;
    public int currentRound;
    public List<RaceData> calendar = new List<RaceData>();
    public List<StandingEntry> driverStandings = new List<StandingEntry>();
    public List<StandingEntry> constructorStandings = new List<StandingEntry>();

    public RaceData NextRace
    {
        get
        {
            foreach (RaceData r in calendar)
            {
                if (!r.completed) return r;
            }
            return null;
        }
    }
}

[Serializable]
public class GameState
{
    public Difficulty difficulty;
    public int currentWeek;
    public int seasonYear;

    public string playerTeamId;
    public List<TeamData> teams = new List<TeamData>();

    public List<DriverData> driverPool = new List<DriverData>();
    public List<EngineerData> engineerPool = new List<EngineerData>();
    public List<StaffData> staffPool = new List<StaffData>();

    public SeasonData season = new SeasonData();

    public TeamData PlayerTeam
    {
        get
        {
            foreach (TeamData t in teams)
            {
                if (t.id == playerTeamId) return t;
            }
            return null;
        }
    }

    public DriverData GetDriver(string id)
    {
        foreach (DriverData d in driverPool)
        {
            if (d.id == id) return d;
        }
        return null;
    }

    public EngineerData GetEngineer(string id)
    {
        foreach (EngineerData e in engineerPool)
        {
            if (e.id == id) return e;
        }
        return null;
    }

    public StaffData GetStaff(string id)
    {
        foreach (StaffData s in staffPool)
        {
            if (s.id == id) return s;
        }
        return null;
    }

    public TeamData GetTeam(string id)
    {
        foreach (TeamData t in teams)
        {
            if (t.id == id) return t;
        }
        return null;
    }
}
