namespace NTSD28Parity;

internal static class B2InputRngJointRawSelfTest
{
    internal static B2InputRngJointRawSelfTestReport Run()
    {
        var cases = new List<B2InputRngJointRawSelfTestCase>();
        RunCase(cases, "equal-capture", () =>
        {
            string authority = Capture("authority-source-model");
            string unity = Capture("unity-diagnostic");
            B2InputRngJointRawValidationReport authorityValidation =
                B2InputRngJointRawContract.ValidateTextForTest(authority);
            B2InputRngJointRawValidationReport unityValidation =
                B2InputRngJointRawContract.ValidateTextForTest(unity);
            B2InputRngJointRawComparisonReport comparison =
                B2InputRngJointRawComparator.CompareTextForTest(
                    authority,
                    unity);
            Require(authorityValidation.Valid, authorityValidation.Reason);
            Require(unityValidation.Valid, unityValidation.Reason);
            Require(comparison.Equal, comparison.FirstDifference?.Path);
        });
        RunCase(cases, "initial-rng-first-difference", () =>
        {
            B2InputRngJointRawComparisonReport comparison =
                B2InputRngJointRawComparator.CompareTextForTest(
                    Capture("authority-source-model"),
                    Capture("unity-diagnostic", initialCrtState: 10));
            Require(!comparison.Equal, "comparison unexpectedly equal");
            Require(
                comparison.FirstDifference?.Path == "initialRng.crt.state",
                comparison.FirstDifference?.Path);
        });
        RunCase(cases, "entity-combo-first-difference", () =>
        {
            B2InputRngJointRawComparisonReport comparison =
                B2InputRngJointRawComparator.CompareTextForTest(
                    Capture("authority-source-model"),
                    Capture("unity-diagnostic", combo3: 1));
            Require(!comparison.Equal, "comparison unexpectedly equal");
            Require(
                comparison.FirstDifference?.Path ==
                    "entities[slot=0].input.comboState[3]",
                comparison.FirstDifference?.Path);
        });
        RunCase(cases, "malformed-combo-rejected", () =>
        {
            B2InputRngJointRawValidationReport validation =
                B2InputRngJointRawContract.ValidateTextForTest(
                    Capture("unity-diagnostic", malformedCombo: true));
            Require(!validation.Valid, "malformed combo was accepted");
        });
        RunCase(cases, "per-call-first-difference", () =>
        {
            B2InputRngJointRawComparisonReport comparison =
                B2InputRngJointRawComparator.CompareTextForTest(
                    CaptureWithSynchronizedCall(
                        "authority-source-model",
                        2),
                    CaptureWithSynchronizedCall(
                        "unity-diagnostic",
                        3));
            Require(!comparison.Equal, "comparison unexpectedly equal");
            Require(
                comparison.FirstDifference?.Path ==
                    "ticks[1].rng.synchronized.calls",
                comparison.FirstDifference?.Path);
        });

        return new B2InputRngJointRawSelfTestReport
        {
            Passed = cases.All(test => test.Passed),
            Cases = cases,
        };
    }

    private static string Capture(
        string producer,
        uint initialCrtState = 9,
        int combo3 = 0,
        bool malformedCombo = false)
    {
        string combo = malformedCombo
            ? "[0,0,0]"
            : $"[0,0,0,{combo3},0,0,0,0,0,0]";
        string template = """
            {"kind":"header","schema":"ntsd28-logan-b2-input-rng-joint-raw-v3","producer":"__PRODUCER__","evidenceClass":"DIAGNOSTIC_ONLY","certificateEligible":false,"formalExeSha256":"B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033","scenarioId":"self-test","firstCompletedTick":1,"expectedTickCount":1,"keyOrder":["W","S","A","D","J","K","L"],"perCallTraceAvailability":{"crt":"available-completed-ticks","synchronized":"available-completed-ticks"},"perCallTraceScope":{"aiCursorExcluded":false,"humanScenarioRequired":false,"initializationExcluded":true},"initialInputPhase":0,"initialRng":{"crt":{"state":__INITIAL_CRT_STATE__,"totalCalls":3000},"synchronized":{"counter":1,"index":1,"tableHash64":"A1BA1B90EA55796D","lastCallSite":4202976,"totalCalls":1}}}
            {"kind":"tick","completedTick":1,"inputPhase":1,"rng":{"crt":{"state":__INITIAL_CRT_STATE__,"totalCalls":3000,"tickCallCount":0,"calls":[]},"synchronized":{"state":{"counter":1,"index":1,"tableHash64":"A1BA1B90EA55796D","lastCallSite":4202976},"totalCalls":1,"tickCallCount":0,"calls":[]}},"entities":[{"slot":0,"allocationEpoch":1,"objectId":2,"input":{"currentMask":17,"previousMask":0,"edgeWindow":{"attack":5,"jump":0,"defend":0,"right":5,"left":0,"up":0,"down":0},"defendReentryCooldown":0,"comboState":__COMBO__,"proxyTail":0,"keyHistory":[-1,-1,-1,6,9],"runAccumulator":0,"lastAction":0,"remapState":0,"remapIndices":[0,1,2,3,4,5,6],"boundState":0,"globalRecordState":0}}]}
            """;
        return template
            .Replace("__PRODUCER__", producer, StringComparison.Ordinal)
            .Replace(
                "__INITIAL_CRT_STATE__",
                initialCrtState.ToString(System.Globalization.CultureInfo.InvariantCulture),
                StringComparison.Ordinal)
            .Replace("__COMBO__", combo, StringComparison.Ordinal) +
            Environment.NewLine;
    }

    private static string CaptureWithSynchronizedCall(
        string producer,
        int upperBound)
    {
        const string oldTick =
            "\"counter\":1,\"index\":1,\"tableHash64\":\"A1BA1B90EA55796D\"," +
            "\"lastCallSite\":4202976},\"totalCalls\":1," +
            "\"tickCallCount\":0,\"calls\":[]";
        string newTick =
            "\"counter\":2,\"index\":2,\"tableHash64\":\"A1BA1B90EA55796D\"," +
            "\"lastCallSite\":130},\"totalCalls\":2," +
            "\"tickCallCount\":1,\"calls\":[{" +
            $"\"callSite\":130,\"upperBound\":{upperBound},\"result\":1," +
            "\"counterAfter\":2,\"indexAfter\":2,\"totalCalls\":2}]";
        return Capture(producer).Replace(
            oldTick,
            newTick,
            StringComparison.Ordinal);
    }

    private static void RunCase(
        ICollection<B2InputRngJointRawSelfTestCase> cases,
        string name,
        Action action)
    {
        try
        {
            action();
            cases.Add(new B2InputRngJointRawSelfTestCase
            {
                Name = name,
                Passed = true,
            });
        }
        catch (Exception exception)
        {
            cases.Add(new B2InputRngJointRawSelfTestCase
            {
                Name = name,
                Passed = false,
                Reason = exception.Message,
            });
        }
    }

    private static void Require(bool condition, string? reason)
    {
        if (!condition)
            throw new InvalidOperationException(reason ?? "assertion failed");
    }
}

internal sealed class B2InputRngJointRawSelfTestReport
{
    public bool Passed { get; set; }
    public List<B2InputRngJointRawSelfTestCase> Cases { get; set; } = new();
}

internal sealed class B2InputRngJointRawSelfTestCase
{
    public string Name { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? Reason { get; set; }
}
