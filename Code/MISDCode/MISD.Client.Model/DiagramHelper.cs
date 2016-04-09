using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISD.Client.Model
{
    public class DiagramHelper
    {
        private static int NUMBER_OF_VALUES_IN_DIAGRAM = 50;

        public static ExtendedObservableCollection<IndicatorValue> filterDiagramValues(IEnumerable<IndicatorValue> values)
        {
            var sortedValues = values.OrderBy(x => x.Timestamp);

            ExtendedObservableCollection<IndicatorValue> resultList = new ExtendedObservableCollection<IndicatorValue>();

            // add average values to the diagram list
            int intervalToAverage = values.Count() / NUMBER_OF_VALUES_IN_DIAGRAM;
            if (intervalToAverage > 1)
            {
                for (int i = 0; i < sortedValues.Count(); i += intervalToAverage)
                {
                    if (sortedValues.Count() >= i + intervalToAverage)
                    {
                        try
                        {
                            double sum = 0;
                            for (int j = i; j < i + intervalToAverage; j++)
                            {
                                sum += Convert.ToInt32(sortedValues.ElementAt(j).Value);
                            }
                            IndicatorValue middleElement = sortedValues.ElementAt(i + intervalToAverage / 2);
                            resultList.Add(new IndicatorValue(Convert.ToInt32(sum / intervalToAverage), middleElement.DataType, middleElement.Timestamp, middleElement.MappingState));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("DiagramHelper.filterDiagramValues: Problem filtering values for diagram. " + e.Message);
                        }
                    }
                }
            }
            else
            {
                foreach (IndicatorValue v in sortedValues)
                {
                    try{
                        resultList.Add(new IndicatorValue(Convert.ToInt32(v.Value), v.DataType, v.Timestamp, v.MappingState));
                    }catch(Exception){}
                }
            }

            // collect distances and define "normal" time distance between two values
            // lets take the median
            List<TimeSpan> distances = new List<TimeSpan>();
            for (int i = 0; i < resultList.Count; i++)
            {
                if (i < resultList.Count - 1)
                {
                    distances.Add((resultList.ElementAt(i + 1).Timestamp - resultList.ElementAt(i).Timestamp));
                }
            }
            //sort and take middle as reference distance
            distances.Sort();
            double referenceDistanceInSeconds = 0;
            if (distances.Count > 0)
            {
                referenceDistanceInSeconds = distances.ElementAt(distances.Count / 2).TotalSeconds;
            }
           
            List<IndicatorValue> valuesToAdd = new List<IndicatorValue>();
            for(int i = 0; i < resultList.Count; i++)
            {
                if (i < resultList.Count - 1)
                {
                    if ((resultList.ElementAt(i + 1).Timestamp - resultList.ElementAt(i).Timestamp).TotalSeconds > referenceDistanceInSeconds * 5)
                    {
                        // Add Dummy 0 Values, so the graph shows no ugly lines when system was off
                        valuesToAdd.Add(new IndicatorValue(0, resultList.ElementAt(i).DataType, resultList.ElementAt(i).Timestamp.AddMilliseconds(100), resultList.ElementAt(i).MappingState));
                        valuesToAdd.Add(new IndicatorValue(0, resultList.ElementAt(i + 1).DataType, resultList.ElementAt(i + 1).Timestamp.AddMilliseconds(-100), resultList.ElementAt(i + 1).MappingState));
                    }
                }
            }

            foreach (IndicatorValue v in valuesToAdd)
            {
                resultList.Add(v);
            }
            return resultList;
        }
    }
}
