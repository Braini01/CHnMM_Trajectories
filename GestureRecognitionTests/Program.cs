using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using GestureRecognitionLib;
using LfS.GestureDatabase;
using System.Diagnostics;
using GestureRecognitionLib.CHnMM.Estimators;
using NDtw;
using SignatureDatabase;
using LfS.GestureRecognitionTests.Experiments;

namespace LfS.GestureRecognitionTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //var param1 = new IntParamVariation("nAreaForStrokeMap", 10, 5, 20);
            var param1 = new IntParamVariation("nAreaForStrokeMap", 0);
            var param2 = new DoubleParamVariation("minRadiusArea", 0.01, 0.02, 0.19);
            var param3 = new DoubleParamVariation("toleranceFactorArea", 1.1, 0.2, 2.1);
            var param4 = new DoubleParamVariation("areaPointDistance", 0.05, 0.1, 0.35);
            //var param4 = new DoubleParamVariation("areaPointDistance", 0);
            var param5 = new BoolParamVariation("useFixAreaNumber", false);
            var param6 = new BoolParamVariation("useSmallestCircle", true);
            var param7 = new BoolParamVariation("isTranslationInvariant", true);
            var param8 = new BoolParamVariation("useAdaptiveTolerance", false);
            var param9 = new DoubleParamVariation("hitProbability", 0.9);
            var param10 = new StringParamVariation("distEstName", new string[]
            {
                nameof(NaiveUniformEstimator),
                //nameof(AdaptiveUniformEstimator),
                //nameof(MinVarianceUniformEstimator),
                nameof(NormalEstimator)
            });

            var configSet = Configuration.getParameterVariations(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);

            int subsets;
            string set;
            bool crossval;

            //Dataset1
            //2 Subsets = 10 Trainingsbeispiele
            subsets = 2;
            set = "OwnForms";
            crossval = true;
            var chnmm = new CHnMMRecognitionExperiment(set, crossval, subsets, configSet);
            var dtw = new DTWExperiment(false, set, crossval, subsets);
            var dtwWithTime = new DTWExperiment(true, set, crossval, subsets);
            var dollar = new DollarExperiment(set, crossval, subsets);

            chnmm.execute();
            //dtw.execute();
            //dtwWithTime.execute();
            //dollar.execute();


            //4 Subsets = 5 Trainingsbeispiele
            subsets = 4;
            chnmm = new CHnMMRecognitionExperiment(set, crossval, subsets, configSet);
            dtw = new DTWExperiment(false, set, crossval, subsets);
            dtwWithTime = new DTWExperiment(true, set, crossval, subsets);
            dollar = new DollarExperiment(set, crossval, subsets);

            chnmm.execute();
            //dtw.execute();
            //dtwWithTime.execute();
            //dollar.execute();

            //=================================================================

            //2 Subsets = 15 Trainingsbeispiele
            subsets = 2;
            set = "TimHard";
            crossval = true;
            chnmm = new CHnMMRecognitionExperiment(set, crossval, subsets, configSet);
            dtw = new DTWExperiment(false, set, crossval, subsets);
            dtwWithTime = new DTWExperiment(true, set, crossval, subsets);
            dollar = new DollarExperiment(set, crossval, subsets);

            chnmm.execute();
            //dtw.execute();
            //dtwWithTime.execute();
            //dollar.execute();

            //6 Subsets = 5 Trainingsbeispiele
            subsets = 6;
            chnmm = new CHnMMRecognitionExperiment(set, crossval, subsets, configSet);
            dtw = new DTWExperiment(false, set, crossval, subsets);
            dtwWithTime = new DTWExperiment(true, set, crossval, subsets);
            dollar = new DollarExperiment(set, crossval, subsets);

            chnmm.execute();
            //dtw.execute();
            //dtwWithTime.execute();
            //dollar.execute();

            //var traces = GestureDatabase.GestureDatabase.getGestureTraces("Tim", "Circle_Slow");

            //var protTrace = Helper.TouchesToGestureTrace(traces[1].Touches , traces[1].Id);
            //var testTrace = Helper.TouchesToGestureTrace(traces[5].Touches, traces[5].Id);

            //var prototype = new GestureRecognitionLib.DTW.Template(protTrace.LongestStroke.Points);

            //var full = new GestureRecognitionLib.DTW.Template(testTrace.LongestStroke.Points);
            //var half = new GestureRecognitionLib.DTW.Template(testTrace.LongestStroke.Points.Where((p, i) => i % 2 == 1));

            //var dtwFull = prototype.getDTW(full);
            //var dtwHalf = prototype.getDTW(half);

            //Console.WriteLine($"DTW Cost (full): {dtwFull.GetCost()}");
            //Console.WriteLine($"DTW Cost (half): {dtwHalf.GetCost()}");

            //var param1 = new IntParamVariation("nAreaForStrokeMap", 10);
            //var param2 = new DoubleParamVariation("minRadiusArea", 0.005, 0.01, 0.025);
            //var param3 = new DoubleParamVariation("toleranceFactorArea", 1.1, 0.1, 1.8);
            //var param4 = new DoubleParamVariation("areaPointDistance", 0.05, 0.1, 0.45);
            //var param5 = new BoolParamVariation("useFixAreaNumber", false);
            //var param6 = new BoolParamVariation("useSmallestCircle");
            //var param7 = new BoolParamVariation("isTranslationInvariant", true);
            //var param8 = new BoolParamVariation("useAdaptiveTolerance");
            //var param9 = new DoubleParamVariation("hitProbability", 0.7, 0.1, 0.9);
            //var param10 = new StringParamVariation("distEstName", new string[]
            //{
            //    //nameof(NaiveUniformEstimator),
            //    //nameof(AdaptiveUniformEstimator),
            //    //nameof(MinVarianceUniformEstimator),
            //    nameof(NormalEstimator)
            //});

            //var param1 = new IntParamVariation("nAreaForStrokeMap", 10);
            //var param2 = new DoubleParamVariation("minRadiusArea", 0.005, 0.01, 0.025);
            //var param3 = new DoubleParamVariation("toleranceFactorArea", 1.4, 0.2, 1.8);
            //var param4 = new DoubleParamVariation("areaPointDistance", 0.05, 0.1, 0.45);
            //var param5 = new BoolParamVariation("useFixAreaNumber", false);
            //var param6 = new BoolParamVariation("useSmallestCircle");
            //var param7 = new BoolParamVariation("isTranslationInvariant", false);
            //var param8 = new BoolParamVariation("useAdaptiveTolerance", false);
            //var param9 = new DoubleParamVariation("hitProbability", 0.7, 0.2, 0.9);
            //var param10 = new StringParamVariation("distEstName", new string[]
            //{
            //    nameof(NaiveUniformEstimator),
            //    //nameof(AdaptiveUniformEstimator),
            //    //nameof(MinVarianceUniformEstimator),
            //    nameof(NormalEstimator)
            //});

            //var param1 = new IntParamVariation("nAreaForStrokeMap", 10);
            //var param2 = new DoubleParamVariation("minRadiusArea", 0.025);
            //var param3 = new DoubleParamVariation("toleranceFactorArea", 1.5);
            //var param4 = new DoubleParamVariation("areaPointDistance", 0.2);
            //var param5 = new BoolParamVariation("useFixAreaNumber", true);
            //var param6 = new BoolParamVariation("useSmallestCircle");
            //var param7 = new BoolParamVariation("isTranslationInvariant");
            //var param8 = new BoolParamVariation("useAdaptiveTolerance");
            //var param9 = new DoubleParamVariation("hitProbability", 0.9);
            //var param10 = new StringParamVariation("distEstName", new string[]
            //{
            //    nameof(NaiveUniformEstimator),
            //    //nameof(AdaptiveUniformEstimator),
            //    //nameof(MinVarianceUniformEstimator),
            //    //nameof(NormalEstimator)
            //});

            //var configSet = Configuration.getParameterVariations(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);


            //var experiment = new Experiment(configSet, "TimAdvanced", 2);

            //var experiment = new Experiment(configSet, "TimSimple", 2);

            //var experiment = new Experiment(configSet, "TimAdvanced", 2);

            //var e0 = new Experiment(configSet, "OwnForms", 4);
            //var e1 = new Experiment(configSet, "TimHard", 2);
            //var e2 = new Experiment(configSet, "TimSimple", 5);

            //var exp = new DTWExperiment(true, "Sigs1", false, 5);


            //var paramaters = new Configuration();
            //paramaters.areaPointDistance = 20;
            //paramaters.isTranslationInvariant = true;
            //paramaters.hitProbability = 0.9;
            //paramaters.minRadiusArea = 5;
            //paramaters.nAreaForStrokeMap = 10;
            //paramaters.toleranceFactorArea = 2;
            //paramaters.useFixAreaNumber = false;
            //paramaters.useSmallestCircle = true;
            //paramaters.distEstName = nameof(NormalEstimator);
            //var exp = new CHnMMExperiment("Sigs1", false, 10, paramaters);

            ////var param1 = new IntParamVariation("nAreaForStrokeMap", 5, 5, 20);
            //var param1 = new IntParamVariation("nAreaForStrokeMap", 0);
            //var param2 = new DoubleParamVariation("minRadiusArea", 0.025, 0.025, 0.15);
            //var param3 = new DoubleParamVariation("toleranceFactorArea", 1.5, 0.5, 3.0);
            //var param4 = new DoubleParamVariation("areaPointDistance", 0.1, 0.1, 0.4);
            ////var param4 = new DoubleParamVariation("areaPointDistance", 0);
            //var param5 = new BoolParamVariation("useFixAreaNumber",false);
            //var param6 = new BoolParamVariation("useSmallestCircle", true);
            //var param7 = new BoolParamVariation("isTranslationInvariant", true);
            //var param8 = new BoolParamVariation("useAdaptiveTolerance", false);
            //var param9 = new DoubleParamVariation("hitProbability", 0.9);
            //var param10 = new StringParamVariation("distEstName", new string[]
            //{
            //    nameof(NaiveUniformEstimator),
            //    //nameof(AdaptiveUniformEstimator),
            //    //nameof(MinVarianceUniformEstimator),
            //    nameof(NormalEstimator)
            //});
            //var configSet = Configuration.getParameterVariations(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10).ToArray();
            ////var exp = new CHnMMRecognitionExperiment("PseudoSigs", false, 10, configSet);
            ////exp.execute();

            //var exp2 = new CHnMMVerificationExperiment("PseudoSigsRandom", 5, false, configSet);
            //exp2.execute();

            //exp2 = new CHnMMVerificationExperiment("PseudoSigsSkilled", 5, false, configSet);
            //exp2.execute();

            //exp2 = new CHnMMVerificationExperiment("DoodlesRandom", 5, false, configSet);
            //exp2.execute();

            //exp2 = new CHnMMVerificationExperiment("DoodlesSkilled", 5, false, configSet);
            //exp2.execute();

            //exp2 = new CHnMMVerificationExperiment("PseudoSigsRandom", 15, configSet);
            //exp2.execute();

            //exp2 = new CHnMMVerificationExperiment("PseudoSigsSkilled", 15, configSet);
            //exp2.execute();

            //exp2 = new CHnMMVerificationExperiment("DoodlesRandom", 15, configSet);
            //exp2.execute();

            //exp2 = new CHnMMVerificationExperiment("DoodlesSkilled", 15, configSet);
            //exp2.execute();


            //var exp = new DTWExperiment(true, "Doodles", false, 10);
            //exp.execute();

            //var data = DataSets.getSignatures(true);

            //Console.Write(data);

            //var dtwRec = new GestureRecognitionLib.DTW.DTWRecognitionSystem(true);
            //var simpleSubsets = new SimpleSplitSubsetCreator(5);
            //var crossvalidationSubsets = new CrossvalidationSubsetCreator(2);
            //var processor = new DynamicAreaPointSampling(0.1);
            //var newExp = new RecognitionExperiment(dtwRec, simpleSubsets, null, "TimHard");

            //var results = newExp.execute();
            //string fileName = "DTWGestureRecognition_TimHard_5trains_withTime.csv";
            //GestureRecognitionResults.saveResultsToFile("..\\..\\ExperimentResults\\" + fileName, results);
        }
    }
}

