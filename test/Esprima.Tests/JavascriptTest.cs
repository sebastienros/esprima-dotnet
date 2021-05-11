using Esprima.Utils;
using Xunit;

namespace Esprima.Tests
{
    public class JavascriptTest
    {
        [Fact]
        public void ToJavascriptTest1()
        {
            var parser = new JavaScriptParser(@"if (true) { p(); }
switch(foo) {
    case 'A':
        p();
        break;
}
switch(foo) {
    default:
        p();
        break;
}
for (var a = []; ; ) { }
for (var elem of list) { }
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
        }

        [Fact]
        public void ToJavascriptTest2()
        {
            var parser = new JavaScriptParser(@"let tips = [
  ""Click on any AST node with a '+' to expand it"",

  ""Hovering over a node highlights the \
   corresponding location in the source code"",

  ""Shift click on an AST node to expand the whole subtree""
];

            function printTips()
            {
                tips.forEach((tip, i) => console.log(`Tip ${ i}:` +tip));
        }");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
        }

        [Fact]
        public void ToJavascriptTest3()
        {
            var parser = new JavaScriptParser(@"export class aa extends HTMLElement{
    constructor(a, b)
    {
        super(a);
        this._div = document.createElement('div');
    }
    static get is() {
        return 'aa';
    }
}");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
        }

        [Fact]
        public void ToJavascriptTest4()
        {
            var parser = new JavaScriptParser(@"import { MccDialog } from '../mccDialogHandler';
import { commonClient, bb as f } from '../commonClient/commonClient';
import ii, { hh, jj } from '../commonClient/commonClient';
import '../commonClient/commonClient';
import aa from 'module-name';
import zz, * as ff from 'module-name';
import * as name from 'module-name';
import('qq');
a++;
--a;
export function checkSecurityAnswerCodeDirect(result) {
    if (!result) {
        MccDialog.warning({
            title: 'SecurityClientErrorOccured',
            message: '<p>internal error, check console</p>',
        });
        return false;
    }
    switch (result.SecurityAnswerCode) {
        case 'Allowed':
            return true;
        case 'Exception':
            MccDialog.warning({
                title: 'SecurityClientInfoTitle',
                message: '<p><t-t>SecurityClientExceptionOccured</t-t></p><p><t-t>Exception</t-t>: <t-t>' + result.Message + '</t-t></p>' + result.StackTrace,
            });
            return false;
        case 'Error':
            MccDialog.warning({
                title: 'SecurityClientErrorOccured',
                message: '<p>' +
                    commonClient.getTranslation('SecurityClientMessage') +
                    ': ' +
                    commonClient.getTranslation(result.Message) +
                    '</p>' +
                    (result.MessageDetails ? '<p><t-t>SecurityClientDetails</t-t>: <t-t>' + result.MessageDetails + '</t-t></p>' : ' '),
            });
            return false;
        default: {
            let messagesnippet = '<p><t-t>SecurityClient_' + result.SecurityAnswerCode + '</t-t></p>';
            if (result.Message !== undefined && result.SecurityAnswerCode === 'LoginFailed') {
                messagesnippet += '\n\n<t-t>SecurityClient_InternalServerErrorMessage</t-t>\n<t-t>' + result.Message + '</t-t>';
            }
            if (result.Role) {
                messagesnippet += '<p><t-t>SecurityClient_CheckedRole</t-t>' + '  [' + result.Role + ']' + '</p>';
            }
            MccDialog.warning({
                title: 'SecurityClientInfoTitle',
                message: messagesnippet,
            });
            return false;
        }
    }
}");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
        }

        [Fact]
        public void ToJavascriptTest5()
        {
            var parser = new JavaScriptParser(@"(function () {
  'use strict';
})();

(class ApplyShimInterface {
  constructor() {
    this.customStyleInterface = null;
    applyShim['invalidCallback'] = ApplyShimUtils.invalidate;
  }
});

(
  a
)();


aa({});

(function aa(){});");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
        }

        [Fact]
        public void ToJavascriptTest6()
        {
            var parser = new JavaScriptParser(@"function _createClass(Constructor, protoProps, staticProps) {
    if (protoProps) _defineProperties(Constructor.prototype, protoProps);
    if (staticProps) _defineProperties(Constructor, staticProps);
    return Constructor;
  }");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
        }

        [Fact]
        public void ToJavascriptTest7()
        {
            var parser = new JavaScriptParser(@"if ((x ? a.nodeName.toLowerCase() === f : 1 === a.nodeType) && ++d && (p && ((i = (o = a[S] || (a[S] = {}))[a.uniqueID] || (o[a.uniqueID] = {}))[h] = [k, d]), a === e))
{
}");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
        }
    }
}
