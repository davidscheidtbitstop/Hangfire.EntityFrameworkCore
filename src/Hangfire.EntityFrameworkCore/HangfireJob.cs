﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hangfire.EntityFrameworkCore
{
    internal class HangfireJob
    {
        public HangfireJob()
        {
            Parameters = new HashSet<HangfireJobParameter>();
            States = new HashSet<HangfireState>();
        }

        public long Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiredAt { get; set; }

        [Required]
        [MaxLength(512)]
        public string ClrType { get; set; }

        [Required]
        [MaxLength(512)]
        public string Method { get; set; }

        public virtual HangfireJobState ActualState { get; set; }

        public virtual ICollection<HangfireJobArgument> Arguments { get; set; }

        public virtual ICollection<HangfireJobParameter> Parameters { get; set; }

        public virtual ICollection<HangfireState> States { get; set; }

        public virtual ICollection<HangfireJobQueue> Queues { get; set; }
    }
}