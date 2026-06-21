using System.Collections.Generic;
using UnityEngine;

public static class WorldGenerator
{
    private static string[] teamNames =
    {
        "Apex Racing", "Velocity GP", "Thunder Motorsport", "Crimson Racing",
        "Nordic Speed", "Solaris GP", "Phoenix Racing", "Titan Motors",
        "Vortex GP", "Eclipse Racing", "Meridian Racing"
    };

    private static string[] firstNames =
    {
        "Lucas","Mateo","Noah","Leo","Elias","Felix","Oscar","Hugo","Liam","Niko",
        "Carlos","Diego","Pablo","Marco","Andre","Kenji","Ravi","Omar","Viktor","Sven",
        "Max","Jack","Ryan","Adam","Theo","Bruno","Dante","Ivan","Pierre","Sergio"
    };

    private static string[] lastNames =
    {
        "Hart","Vale","Sterling","Rossi","Lindqvist","Moreau","Becker","Costa","Tanaka","Novak",
        "Reyes","Fischer","Albon","Vettori","Kowalski","Petrov","Larsson","Mendez","Brandt","Suzuki",
        "Carter","Walsh","Nguyen","Dubois","Ferri","Schmidt","Andersen","Marquez","Volkov","Okafor"
    };

    private static string[] trackNames =
    {
        "Silverstone","Monza","Spa","Suzuka","Interlagos","Monaco","Sakhir","Melbourne",
        "Barcelona","Zandvoort","Austin","Singapore","Hungaroring","Imola","Montreal","Jeddah",
        "Mexico City","Baku","Shanghai","Portimao","Mugello","Estoril"
    };

    private static string[] countries =
    {
        "UK","Italy","Belgium","Japan","Brazil","Monaco","Bahrain","Australia",
        "Spain","Netherlands","USA","Singapore","Hungary","Italy","Canada","Saudi Arabia",
        "Mexico","Azerbaijan","China","Portugal","Italy","Portugal"
    };

    public static GameState Generate(Difficulty difficulty)
    {
        GameState state = new GameState();
        state.difficulty = difficulty;
        state.currentWeek = 1;
        state.seasonYear = 2025;

        long startMoney = GetStartMoney(difficulty);
        int philosophyCount = System.Enum.GetValues(typeof(TeamPhilosophy)).Length;

        for (int i = 0; i < teamNames.Length; i++)
        {
            TeamData team = new TeamData();
            team.id = "team_" + i;
            team.teamName = teamNames[i];
            team.isPlayer = (i == 0);
            team.philosophy = (TeamPhilosophy)(i % philosophyCount);

            if (team.isPlayer)
            {
                team.money = startMoney;
                team.reputation = 20;
            }
            else
            {
                team.money = Random.Range(40, 121) * 1000000L;
                team.reputation = Random.Range(30, 86);
            }

            SetupTeamCars(team);
            SetupDepartments(team);
            SetupTechTrees(team);
            SetupFacilities(team, team.isPlayer);

            team.aiCarPerformance = team.isPlayer ? 50f : Random.Range(55f, 88f);

            state.teams.Add(team);
        }

        state.playerTeamId = "team_0";

        GenerateDrivers(state);
        GenerateEngineers(state);
        GenerateStaff(state);
        GenerateSeason(state);

        return state;
    }

    private static long GetStartMoney(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.Easy: return 150000000L;
            case Difficulty.Normal: return 80000000L;
            case Difficulty.Hard: return 40000000L;
            case Difficulty.Extreme: return 15000000L;
        }
        return 80000000L;
    }

    private static void SetupTeamCars(TeamData team)
    {
        float baseLevel = team.isPlayer ? 42f : Random.Range(45f, 85f);
        for (int c = 0; c < 2; c++)
        {
            CarData car = new CarData();
            car.id = team.id + "_car_" + c;
            car.carName = team.teamName + " " + (c == 0 ? "01" : "02");
            float v = baseLevel + Random.Range(-5f, 5f);
            car.frontWing = v;
            car.rearWing = v;
            car.floor = v;
            car.enginePower = v;
            car.engineEfficiency = v;
            car.weight = Mathf.Clamp(100f - v, 20f, 80f);
            car.balance = v;
            car.suspension = v;
            car.reliability = Mathf.Clamp(v + Random.Range(0f, 15f), 40f, 100f);
            car.downforceSetup = 0.5f;
            car.balanceSetup = 0.5f;
            car.reliabilitySetup = 0.5f;
            team.cars.Add(car);
        }
    }

    private static void SetupDepartments(TeamData team)
    {
        foreach (DepartmentType dt in System.Enum.GetValues(typeof(DepartmentType)))
        {
            DepartmentData dep = new DepartmentData();
            dep.type = dt;
            dep.level = team.isPlayer ? 1 : Random.Range(1, 5);
            dep.outputRating = dep.level * 20f;
            team.departments.Add(dep);
        }
    }

    private static void SetupTechTrees(TeamData team)
    {
        string[][] nodeNames = new string[][]
        {
            new string[] { "Basic Wing", "Optimized Wing", "Ground Effect", "Advanced Airflow", "Experimental Aero" },
            new string[] { "Power Unit", "Efficiency", "Cooling System", "Engine Reliability", "Hybrid Boost" },
            new string[] { "Carbon Structure", "Light Alloy", "Composite Panels", "Nano Coating", "Exotic Compounds" },
            new string[] { "Basic Sensors", "Data Analysis", "Control Systems", "Adaptive ECU", "Predictive AI" }
        };

        int treeIndex = 0;
        foreach (TechTreeType tt in System.Enum.GetValues(typeof(TechTreeType)))
        {
            TechTree tree = new TechTree();
            tree.type = tt;
            for (int lvl = 0; lvl < 5; lvl++)
            {
                TechNode node = new TechNode();
                node.id = team.id + "_" + tt + "_" + lvl;
                node.nodeName = nodeNames[treeIndex][lvl];
                node.level = lvl + 1;
                node.unlocked = team.isPlayer ? (lvl == 0) : (lvl <= Random.Range(0, 3));
                node.unlockCost = (lvl + 1) * 5000000;
                node.performanceBonus = (lvl + 1) * 3f;
                tree.nodes.Add(node);
            }
            team.techTrees.Add(tree);
            treeIndex++;
        }
    }

    private static void SetupFacilities(TeamData team, bool isPlayer)
    {
        foreach (FacilityType ft in System.Enum.GetValues(typeof(FacilityType)))
        {
            FacilityData f = new FacilityData();
            f.type = ft;
            f.level = isPlayer ? 1 : Random.Range(1, 6);
            f.upgradeCost = (f.level + 1) * 8000000;
            team.facilities.Add(f);
        }
    }

    private static void GenerateDrivers(GameState state)
    {
        int idx = 0;
        for (int t = 0; t < state.teams.Count; t++)
        {
            TeamData team = state.teams[t];
            for (int s = 0; s < 2; s++)
            {
                DriverData d = CreateDriver(idx, team.isPlayer);
                d.teamId = team.id;
                team.driverIds.Add(d.id);
                team.raceDriverIds[s] = d.id;
                state.driverPool.Add(d);
                idx++;
            }
        }
        for (int f = 0; f < 8; f++)
        {
            DriverData d = CreateDriver(idx, false);
            d.teamId = "";
            state.driverPool.Add(d);
            idx++;
        }
        for (int a = 0; a < 6; a++)
        {
            DriverData d = CreateDriver(idx, false);
            d.teamId = "";
            d.isAcademy = true;
            d.age = Random.Range(17, 21);
            d.potential = Random.Range(85, 99);
            state.driverPool.Add(d);
            idx++;
        }
    }

    private static DriverData CreateDriver(int index, bool playerTeam)
    {
        DriverData d = new DriverData();
        d.id = "driver_" + index;
        d.firstName = firstNames[Random.Range(0, firstNames.Length)];
        d.lastName = lastNames[Random.Range(0, lastNames.Length)];
        d.age = Random.Range(19, 38);
        int baseSkill = playerTeam ? Random.Range(56, 70) : Random.Range(55, 92);
        d.speed = Clamp100(baseSkill + Random.Range(-8, 9));
        d.qualifying = Clamp100(baseSkill + Random.Range(-8, 9));
        d.consistency = Clamp100(baseSkill + Random.Range(-8, 9));
        d.tireManagement = Clamp100(baseSkill + Random.Range(-8, 9));
        d.feedback = Clamp100(baseSkill + Random.Range(-8, 9));
        d.aggression = Clamp100(Random.Range(40, 95));
        d.wetSkill = Clamp100(baseSkill + Random.Range(-10, 11));
        d.potential = Clamp100(Mathf.Max(baseSkill, 100 - d.age + Random.Range(0, 20)));
        d.salary = baseSkill * 100000;
        d.contractYears = Random.Range(1, 4);
        return d;
    }

    private static void GenerateEngineers(GameState state)
    {
        int idx = 0;
        Specialization[] specs = (Specialization[])System.Enum.GetValues(typeof(Specialization));
        for (int t = 0; t < state.teams.Count; t++)
        {
            TeamData team = state.teams[t];
            int count = team.isPlayer ? 5 : Random.Range(5, 9);
            for (int e = 0; e < count; e++)
            {
                EngineerData eng = CreateEngineer(idx, team.isPlayer, specs);
                eng.teamId = team.id;
                eng.isAssigned = true;
                eng.assignedDepartment = (DepartmentType)(e % 5);
                DepartmentData dep = FindDepartment(team, eng.assignedDepartment);
                dep.assignedEngineerIds.Add(eng.id);
                team.engineerIds.Add(eng.id);
                state.engineerPool.Add(eng);
                idx++;
            }
        }
        for (int f = 0; f < 10; f++)
        {
            EngineerData eng = CreateEngineer(idx, false, specs);
            eng.teamId = "";
            state.engineerPool.Add(eng);
            idx++;
        }
    }

    private static EngineerData CreateEngineer(int index, bool playerTeam, Specialization[] specs)
    {
        EngineerData e = new EngineerData();
        e.id = "eng_" + index;
        e.firstName = firstNames[Random.Range(0, firstNames.Length)];
        e.lastName = lastNames[Random.Range(0, lastNames.Length)];
        e.age = Random.Range(28, 60);
        int baseSkill = playerTeam ? Random.Range(48, 66) : Random.Range(50, 90);
        e.skill = Clamp100(baseSkill + Random.Range(-6, 7));
        e.creativity = Clamp100(Random.Range(40, 95));
        e.experience = Clamp100(Mathf.Min(95, (e.age - 25) * 3 + Random.Range(0, 20)));
        e.specialization = specs[Random.Range(0, specs.Length)];
        e.salary = baseSkill * 60000;
        e.contractYears = Random.Range(1, 4);
        return e;
    }

    private static DepartmentData FindDepartment(TeamData team, DepartmentType type)
    {
        for (int i = 0; i < team.departments.Count; i++)
            if (team.departments[i].type == type) return team.departments[i];
        return team.departments[0];
    }

    private static void GenerateStaff(GameState state)
    {
        int idx = 0;
        for (int t = 0; t < state.teams.Count; t++)
        {
            TeamData team = state.teams[t];
            StaffData principal = CreateStaff(idx, StaffRole.TeamPrincipal, team.isPlayer);
            principal.teamId = team.id;
            team.teamPrincipalId = principal.id;
            state.staffPool.Add(principal);
            idx++;

            StaffData td = CreateStaff(idx, StaffRole.TechnicalDirector, team.isPlayer);
            td.teamId = team.id;
            team.technicalDirectorId = td.id;
            state.staffPool.Add(td);
            idx++;
        }
        for (int f = 0; f < 4; f++)
        {
            StaffData s = CreateStaff(idx, f % 2 == 0 ? StaffRole.TeamPrincipal : StaffRole.TechnicalDirector, false);
            s.teamId = "";
            state.staffPool.Add(s);
            idx++;
        }
    }

    private static StaffData CreateStaff(int index, StaffRole role, bool playerTeam)
    {
        StaffData s = new StaffData();
        s.id = "staff_" + index;
        s.firstName = firstNames[Random.Range(0, firstNames.Length)];
        s.lastName = lastNames[Random.Range(0, lastNames.Length)];
        s.role = role;
        int baseSkill = playerTeam ? Random.Range(48, 64) : Random.Range(50, 90);
        s.skill = Clamp100(baseSkill);
        s.experience = Clamp100(Random.Range(40, 95));
        s.salary = baseSkill * 80000;
        s.contractYears = Random.Range(1, 4);
        return s;
    }

    private static void GenerateSeason(GameState state)
    {
        state.season = new SeasonData();
        state.season.year = state.seasonYear;
        state.season.currentRound = 1;
        for (int i = 0; i < 22; i++)
        {
            RaceData race = new RaceData();
            race.id = "race_" + i;
            race.round = i + 1;
            race.trackName = trackNames[i % trackNames.Length];
            race.country = countries[i % countries.Length];
            race.laps = Random.Range(44, 71);
            race.trackLengthKm = Random.Range(38, 71) / 10f + 3f;
            race.weatherWetChance = Random.Range(0f, 0.4f);
            state.season.calendar.Add(race);
        }
        for (int t = 0; t < state.teams.Count; t++)
        {
            StandingEntry cs = new StandingEntry();
            cs.id = state.teams[t].id;
            state.season.constructorStandings.Add(cs);
        }
        for (int i = 0; i < state.driverPool.Count; i++)
        {
            DriverData d = state.driverPool[i];
            if (!string.IsNullOrEmpty(d.teamId))
            {
                StandingEntry ds = new StandingEntry();
                ds.id = d.id;
                state.season.driverStandings.Add(ds);
            }
        }
    }

    private static int Clamp100(int v)
    {
        return Mathf.Clamp(v, 1, 99);
    }

    private const long SeasonPrizeTop = 50000000L;
    private const long SeasonPrizeBottom = 5000000L;

    public static long SeasonPrize(int posZeroBased, int teamCount)
    {
        float frac = teamCount > 1 ? posZeroBased / (float)(teamCount - 1) : 0f;
        return (long)Mathf.Lerp(SeasonPrizeTop, SeasonPrizeBottom, frac);
    }

    public static void RolloverSeason(GameState state)
    {
        List<StandingEntry> finalC = new List<StandingEntry>(state.season.constructorStandings);
        finalC.Sort((a, b) => b.points.CompareTo(a.points));
        int teamCount = Mathf.Max(finalC.Count, 1);
        for (int pos = 0; pos < finalC.Count; pos++)
        {
            TeamData team = FindTeam(state, finalC[pos].id);
            if (team == null) continue;
            team.money += SeasonPrize(pos, teamCount);
            if (team.id == state.playerTeamId)
            {
                int repDelta = Mathf.Clamp(6 - pos * 2, -3, 8);
                team.reputation = Mathf.Clamp(team.reputation + repDelta, 0, 100);
            }
        }

        for (int t = 0; t < state.teams.Count; t++)
        {
            TeamData team = state.teams[t];
            if (team.id == state.playerTeamId) continue;
            float bump = Random.Range(2f, 6f);
            for (int c = 0; c < team.cars.Count; c++)
            {
                CarData car = team.cars[c];
                car.frontWing = ClampF(car.frontWing + bump);
                car.rearWing = ClampF(car.rearWing + bump);
                car.floor = ClampF(car.floor + bump);
                car.enginePower = ClampF(car.enginePower + bump);
                car.engineEfficiency = ClampF(car.engineEfficiency + bump);
                car.balance = ClampF(car.balance + bump);
                car.suspension = ClampF(car.suspension + bump);
                car.reliability = ClampF(car.reliability + bump);
            }
            team.aiCarPerformance = Mathf.Clamp(team.aiCarPerformance + bump, 0f, 100f);
        }

        for (int i = 0; i < state.driverPool.Count; i++)
        {
            DriverData d = state.driverPool[i];
            d.age += 1;
            d.contractYears = Mathf.Max(1, d.contractYears - 1);
        }
        for (int i = 0; i < state.engineerPool.Count; i++)
        {
            EngineerData e = state.engineerPool[i];
            e.age += 1;
            e.contractYears = Mathf.Max(1, e.contractYears - 1);
        }
        for (int i = 0; i < state.staffPool.Count; i++)
        {
            StaffData s = state.staffPool[i];
            s.contractYears = Mathf.Max(1, s.contractYears - 1);
        }

        for (int t = 0; t < state.teams.Count; t++)
        {
            state.teams[t].seasonWins = 0;
            state.teams[t].championshipPoints = 0;
        }
        for (int i = 0; i < state.season.constructorStandings.Count; i++)
        {
            state.season.constructorStandings[i].points = 0;
            state.season.constructorStandings[i].wins = 0;
        }
        for (int i = 0; i < state.season.driverStandings.Count; i++)
        {
            state.season.driverStandings[i].points = 0;
            state.season.driverStandings[i].wins = 0;
        }

        state.seasonYear += 1;
        state.currentWeek = 1;
        state.season.year = state.seasonYear;
        state.season.currentRound = 1;
        state.season.calendar.Clear();
        for (int i = 0; i < 22; i++)
        {
            RaceData race = new RaceData();
            race.id = "race_" + i;
            race.round = i + 1;
            race.trackName = trackNames[i % trackNames.Length];
            race.country = countries[i % countries.Length];
            race.laps = Random.Range(44, 71);
            race.trackLengthKm = Random.Range(38, 71) / 10f + 3f;
            race.weatherWetChance = Random.Range(0f, 0.4f);
            state.season.calendar.Add(race);
        }
    }

    private static TeamData FindTeam(GameState state, string id)
    {
        for (int i = 0; i < state.teams.Count; i++) if (state.teams[i].id == id) return state.teams[i];
        return null;
    }

    private static float ClampF(float v)
    {
        return Mathf.Clamp(v, 0f, 100f);
    }
}
