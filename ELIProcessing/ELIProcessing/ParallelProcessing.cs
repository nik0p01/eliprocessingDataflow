using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ELIProcessing
{
    static class ParallelProcessing
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static PixelsForTask[] _pixelsForTasks;
        private static ushort[] _result;

        static private BufferBlock<PixelsForTask> _PixelsForTaskBufferBlock;
        static private ActionBlock<PixelsForTask> _calcBlock;

        static void CalcBlock(PixelsForTask pixelsForTask)
        {
            int i = 0;
            for (int j = pixelsForTask.indexStart; j < pixelsForTask.indexStart + pixelsForTask.pixelsFirst.Length; j++)
            {
                _result[j] = (ushort)(pixelsForTask.pixelsSecond[i] == 0 ? 0 : (pixelsForTask.pixelsFirst[i] / pixelsForTask.pixelsSecond[i]));
                i++;
            }

            _logger.Trace("Method CalcBlock worked witch index " + pixelsForTask.indexStart);
        }

        static public ICollection<ushort> ParallelProcessingRun(string nameFirst, string nameSecond, int CountPixelsInTask, out int imageWidth, out int imageHeight)
        {
            imageWidth = 0;
            imageHeight = 0;
            int processorCount = Environment.ProcessorCount;
            _PixelsForTaskBufferBlock = new BufferBlock<PixelsForTask>();
            _calcBlock = new ActionBlock<PixelsForTask>(
                 pixelsForTask => CalcBlock(pixelsForTask),
                 new ExecutionDataflowBlockOptions
                 {
                     MaxDegreeOfParallelism = processorCount
                 });
            _PixelsForTaskBufferBlock.LinkTo(_calcBlock);
            if (File.Exists(nameFirst) && File.Exists(nameSecond))
            {
                using (BinaryReader readerFirst = new BinaryReader(File.Open(nameFirst, FileMode.Open)))
                {
                    using (BinaryReader readerSecond = new BinaryReader(File.Open(nameSecond, FileMode.Open)))
                    {
                        int dataOffsetFirst = WorkWithHeader(readerFirst, out imageWidth, out imageHeight);
                        int dataOffsetSecond = WorkWithHeader(readerSecond, out imageWidth, out imageHeight);
                        int countPixels = (int)((new FileInfo(nameFirst)).Length - dataOffsetFirst) / 2;
                        _result = new ushort[countPixels];
                        int countParts = countPixels % CountPixelsInTask == 0 ? countPixels / CountPixelsInTask : countPixels / CountPixelsInTask + 1;
                        for (int i = 0; i < countParts; i++)
                        {
                            PixelsForTask pixelsForTask = new PixelsForTask() { indexStart = i * CountPixelsInTask };
                            CountPixelsInTask = i == (countParts - 1) ? countPixels % CountPixelsInTask : CountPixelsInTask;
                            pixelsForTask.pixelsFirst = new ushort[CountPixelsInTask];
                            pixelsForTask.pixelsSecond = new ushort[CountPixelsInTask];
                            for (int j = 0; j < CountPixelsInTask; j++)
                            {
                                pixelsForTask.pixelsFirst[j] = (readerFirst.ReadUInt16());
                            }
                            for (int j = 0; j < CountPixelsInTask; j++)
                            {
                                pixelsForTask.pixelsSecond[j] = (readerSecond.ReadUInt16());
                            }
                            _PixelsForTaskBufferBlock.Post(pixelsForTask);
                            _logger.Trace("Method ParallelProcessingRun worked witch index " + pixelsForTask.indexStart);
                        }
                    }
                }
                _PixelsForTaskBufferBlock.Complete();
                return _result;
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        static private int WorkWithHeader(BinaryReader reader, out int imageWidth, out int imageHeight)
        {
            int dataOffset = -1;
            int offset = 0;
            imageWidth = 0;
            imageHeight = 0;
            while (true)
            {
                int value = reader.ReadInt32();

                if (offset == 8)
                {
                    dataOffset = value;
                }
                else if (offset == 16)
                {
                    imageWidth = value;
                }
                else if (offset == 20)
                {
                    imageHeight = value;
                }

                offset += 4;
                if (offset == dataOffset)
                {
                    break;
                }
            }
            return dataOffset;
        }
    }
}
