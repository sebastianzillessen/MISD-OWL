/*
* Copyright 2012 Paul Brombosch, Ehssan Doust, David Krauss,
* Fabian Müller, Yannic Noller, Hanna Schäfer, Jonas Scheurich,
* Arno Schneider, Sebastian Zillessen
*
* This file is part of MISD-OWL, a project of the
* University of Stuttgart (Institution VISUS, Studienprojekt Spring 2012).
*
* MISD-OWL is published under GNU Lesser General Public License Version 3.
* MISD-OWL is free software, you are allowed to redistribute and/or
* modify it under the terms of the GNU Lesser General Public License
* Version 3 or any later version. For details see here:
* http://www.gnu.org/licenses/lgpl.html
*
* MISD-OWL is distributed without any warranty, without even the
* implied warranty of merchantability or fitness for a particular purpose.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Runspaces;

namespace MISD.Server.Cluster
{
    /// <summary>
    /// Represents a pipeline with the according number.
    /// This class is needed for the pipeline-management in the HpcClusterConnection.
    /// </summary>
    public class HpcPipelineObject : IDisposable
    {
        public int pipelineNumber { get; set; }
        public Pipeline pipeline { get; set; }

        public HpcPipelineObject(int pipelineNumber, Pipeline pipeline)
        {
            this.pipelineNumber = pipelineNumber;
            this.pipeline = pipeline;
        }

        public void Dispose()
        {
            pipeline.Dispose();
        }
    }
}
