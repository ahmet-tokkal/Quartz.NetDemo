using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace QuartzDemo
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Belirli bir Tarih için -> 1");
            Console.WriteLine("Belirli bir tarihte belirli aralıklarla 10 defa için -> 2");
            Console.WriteLine("Şuandan itibaren belirli aralıklarla sonsuza kadar için -> 3");
            Console.WriteLine("Şuandan itibaren belirli aralıklarla belirtilen zamana kadar için -> 4");
            Console.WriteLine("Hergün belirli bir saat için -> 5");
            Console.WriteLine("Her Çarşamba belirtilen saat için -> 6");
            Console.WriteLine("Günlük belirli bir saat için -> 7");
            Console.WriteLine("Her gün 8am ve 5pm arası her dakika -> 8");
            Console.WriteLine("3.Cuma belirli saate -> 9");
            Console.WriteLine("Belirtilen zaman kaçırılırsa çeşitli senaryo uygulama -> 10");
            Console.WriteLine();
            Console.Write("Option :");
            int option = Convert.ToInt32(Console.ReadLine());
            RunProgram(option).GetAwaiter().GetResult();
            Console.WriteLine("Press any key to close the application");
            Console.ReadKey();

        }
        private static async Task RunProgram(int option)
        {
            try
            {
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);

                IScheduler scheduler = await factory.GetScheduler();

                await scheduler.Start();
                IJobDetail job = JobBuilder.Create<HelloJob>()
                                            .WithIdentity("job1", "group1")
                                            .UsingJobData("message", "Hello World!")
                                            .UsingJobData("myFloatValue", 3.141f)
                                            .Build();

                //Varsayılan
                ITrigger trigger;
                switch (option)
                {
                    case 1:
                        //Belirli bir tarih
                        DateTime dateTime = new DateTime(2018, 8, 1);
                        trigger = TriggerBuilder.Create()
                                .WithIdentity("trigger1", "group1")
                                .StartAt(dateTime).Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 2:
                        //Belirli bir tarihte belirli aralıklarla 10 defa
                        DateTime dt = new DateTime(2018, 8, 1);
                        trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger1", "group1")
                            .WithSimpleSchedule(x => x
                                .WithIntervalInSeconds(10)
                                .WithRepeatCount(10))
                            .StartAt(dt)
                            .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 3:
                        //Şuandan itibaren belirli aralıklarla sonsuza kadar.
                        trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger1", "group1")
                            .StartNow()
                            .WithSimpleSchedule(x => x
                                .WithIntervalInSeconds(10)
                                .RepeatForever())
                            .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 4:
                        //Şuandan itibaren belirli aralıklarla belirtilen zamana kadar.
                        trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger1", "group1")
                            .StartNow()
                            .WithSimpleSchedule(x => x
                                .WithIntervalInSeconds(10)
                                .RepeatForever())
                            .EndAt(DateBuilder.DateOf(23, 0, 0))
                            .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 5:
                        //Hergün saat 8.43 te
                        trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger1", "group1")
                            .WithDailyTimeIntervalSchedule(s => s
                                .OnEveryDay()
                                .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(8, 43)))
                            .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    /* Cron Expressions
                     * "Saniye Dakika Saat AyınGünü Ay HaftanınGünü Yıl[Opsiyonel]"
                     * Birden fazla gün için MON-FRI
                     * ? özel bir değer olmadığını belirtir.
                     * * her bir seçenek olabilir anlamına gelir
                     * # ile üçüncü cuma gibi bir zamanlamada FRI#3 diyerek yapılabilinir.
                     */
                    case 6:
                        //Her Çarşamba belirtilen saatte
                        trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger1", "group1")
                            .WithCronSchedule("0 0 12 ? * WED")
                            .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 7:
                        //Günlük belirli saate
                        trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger1", "group1")
                            .WithCronSchedule("0 42 10 * * ?")
                            .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 8:
                        //Her gün 8am ve 5pm arası her dakika
                        trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger1", "group1")
                            .WithCronSchedule("0 0/2 8-17 * * ?")
                            .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 9:
                        //3.Cuma belirli saate
                        trigger = TriggerBuilder.Create()
                           .WithIdentity("trigger1", "group1")
                           .WithCronSchedule("0 0 12 * * FRI#3")
                           .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                    case 10:
                        //Belirtilen zaman kaçırılırsa çeşitli senaryo uygulama
                        trigger = TriggerBuilder.Create()
                           .WithIdentity("trigger1", "group1")
                           .WithCronSchedule("0 0 12 * * FRI#3", x => x
                                .WithMisfireHandlingInstructionFireAndProceed())
                           .Build();
                        await scheduler.ScheduleJob(job, trigger);
                        break;
                }
                //Programın ne kadar çalışacağının ayarlanması
                await Task.Delay(TimeSpan.FromSeconds(100));

                await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }
        public class HelloJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                JobKey key = context.JobDetail.Key;
                JobDataMap dataMap = context.JobDetail.JobDataMap;

                string message = dataMap.GetString("message");
                float myFloatValue = dataMap.GetFloat("myFloatValue");

                await Console.Error.WriteLineAsync(message + " : " + myFloatValue);
            }
        }
    }
}

