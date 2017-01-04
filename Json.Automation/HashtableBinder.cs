using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;


namespace Json.Automation {
public class HashtableBinder {
        protected PSCmdlet _context;
        public HashtableBinder(PSCmdlet context) {
            _context = context;
        }
    }
}
