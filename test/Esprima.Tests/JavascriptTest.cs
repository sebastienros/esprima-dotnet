using System;
using System.Text.RegularExpressions;
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

            Assert.Equal("if(true){p();}switch(foo){case 'A':p();break;}switch(foo){default:p();break;}for(var a=[];;){}for(var elem of list){}", code);
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
            Assert.Equal("let tips=[\"Click on any AST node with a '+' to expand it\",\"Hovering over a node highlights the \\\r\n   corresponding location in the source code\",\"Shift click on an AST node to expand the whole subtree\"];function printTips(){tips.forEach((tip,i)=>console.log((`Tip ${i}:`+tip)));}", code);
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
            Assert.Equal("export class aa extends HTMLElement{constructor(a,b){super(a);this._div=(document.createElement('div'));}static get is(){return 'aa';}}", code);
        }

        [Fact]
        public void ToJavascriptTest4()
        {
            var source = @"import { MccDialog } from '../mccDialogHandler';
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
}";
            source = Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program, true);

            var expected = @"import { MccDialog } from '../mccDialogHandler';
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
    if (!(result)) {
        MccDialog.warning({
            title : 'SecurityClientErrorOccured',
            message : '<p>internal error, check console</p>'
        });
        return false;
    }
    switch(result.SecurityAnswerCode) {
        case 'Allowed':
            return true;
        case 'Exception':
            MccDialog.warning({
                title : 'SecurityClientInfoTitle',
                message : ((('<p><t-t>SecurityClientExceptionOccured</t-t></p><p><t-t>Exception</t-t>: <t-t>' + result.Message) + '</t-t></p>') + result.StackTrace)
            });
            return false;
        case 'Error':
            MccDialog.warning({
                title : 'SecurityClientErrorOccured',
                message : ((((('<p>' + (commonClient.getTranslation('SecurityClientMessage'))) + ': ') + (commonClient.getTranslation(result.Message))) + '</p>') + (result.MessageDetails?(('<p><t-t>SecurityClientDetails</t-t>: <t-t>' + result.MessageDetails) + '</t-t></p>'):' '))
            });
            return false;
        default:
            {
                let messagesnippet = (('<p><t-t>SecurityClient_' + result.SecurityAnswerCode) + '</t-t></p>');
                if ((result.Message !== undefined) && (result.SecurityAnswerCode === 'LoginFailed')) {
                    messagesnippet += (('\n\n<t-t>SecurityClient_InternalServerErrorMessage</t-t>\n<t-t>' + result.Message) + '</t-t>');
                }
                if (result.Role) {
                    messagesnippet += (((('<p><t-t>SecurityClient_CheckedRole</t-t>' + '  [') + result.Role) + ']') + '</p>');
                }
                MccDialog.warning({
                    title : 'SecurityClientInfoTitle',
                    message : messagesnippet
                });
                return false;
            }
    }
}";
            expected = Regex.Replace(expected, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            Assert.Equal(expected, code);
        }

        [Fact]
        public void ToJavascriptTest5()
        {
            var source = @"(function () {
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

(function aa(){});";
            source = Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program, true);

            var expected = @"(function() {
    'use strict';
})();
(class ApplyShimInterface{constructor() {
    this.customStyleInterface = null;
    applyShim['invalidCallback'] = ApplyShimUtils.invalidate;
}});
a();
aa({});
function aa() {
    
};";
            expected = Regex.Replace(expected, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            Assert.Equal(expected, code);
        }

        [Fact]
        public void ToJavascriptTest6()
        {
            var source = @"function _createClass(Constructor, protoProps, staticProps) {
    if (protoProps) _defineProperties(Constructor.prototype, protoProps);
    if (staticProps) _defineProperties(Constructor, staticProps);
    return Constructor;
  }";
            source = Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("function _createClass(Constructor,protoProps,staticProps){if(protoProps)_defineProperties(Constructor.prototype,protoProps);if(staticProps)_defineProperties(Constructor,staticProps);return Constructor;}", code);
        }

        [Fact]
        public void ToJavascriptTest7()
        {
            var parser = new JavaScriptParser(@"if ((x ? a.nodeName.toLowerCase() === f : 1 === a.nodeType) && ++d && (p && ((i = (o = a[S] || (a[S] = {}))[a.uniqueID] || (o[a.uniqueID] = {}))[h] = [k, d]), a === e))
{
}");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("if(((x?((a.nodeName.toLowerCase())===f):(1===a.nodeType))&&(++d))&&(p&&((i=((o=(a[S]||(a[S]={})))[a.uniqueID]||(o[a.uniqueID]={})))[h]=[k,d]),a===e)){}", code);
        }

        [Fact]
        public void ToJavascriptTest8()
        {
            var parser = new JavaScriptParser(@"
class a extends b {
    constructor() {
        super();
        this.g=1;
    }

    q=1;
    r='cc';
}
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("class a extends b{constructor(){super();this.g=1;}q=1;r='cc';}", code);
        }

        [Fact]
        public void ToJavascriptTest9()
        {
            var parser = new JavaScriptParser(@"
d = (s = (r = (i = (o = (a = c)[S] || (a[S] = {}))[a.uniqueID] || (o[a.uniqueID] = {}))[h] || [])[0] === k && r[1]) && r[2], a = s && c.childNodes[s];
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("d=((s=(((r=((i=((o=((a=c)[S]||(a[S]={})))[a.uniqueID]||(o[a.uniqueID]={})))[h]||[]))[0]===k)&&r[1]))&&r[2]),a=(s&&c.childNodes[s]);", code);
        }

        [Fact]
        public void ToJavascriptTest10()
        {
            var parser = new JavaScriptParser(@"
m = (z.document, !!v.documentElement && !!v.head && 'function' == typeof v.addEventListener && v.createElement, ~a.indexOf('MSIE') || a.indexOf('Trident/'), '___FONT_AWESOME___')
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("m=(z.document,(((!!(v.documentElement))&&(!!(v.head)))&&('function'==(typeof (v.addEventListener))))&&v.createElement,(~(a.indexOf('MSIE')))||(a.indexOf('Trident/')),'___FONT_AWESOME___');", code);
        }

        [Fact]
        public void ToJavascriptTest11()
        {
            var parser = new JavaScriptParser(@"
 var h = (c.navigator || {}).userAgent,
        a = void 0 === h ? '' : h,
        z = c,
        v = l,
        m = (z.document, !!v.documentElement && !!v.head && 'function' == typeof v.addEventListener && v.createElement, ~a.indexOf('MSIE') || a.indexOf('Trident/'), '___FONT_AWESOME___'),
        e = function() {
            try {
                return !0
            } catch (c) {
                return !1
            }
        }();
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("var h=(c.navigator||{}).userAgent,a=((void 0)===h?'':h),z=c,v=l,m=(z.document,(((!!(v.documentElement))&&(!!(v.head)))&&('function'==(typeof (v.addEventListener))))&&v.createElement,(~(a.indexOf('MSIE')))||(a.indexOf('Trident/')),'___FONT_AWESOME___'),e=((function(){try {return !0;} catch(c){return !1;}})());", code);
        }

        [Fact]
        public void ToJavascriptTest12()
        {
            var parser = new JavaScriptParser(@"
var a = {
children: (b = O, 'g' === b.tag ? b.children : [b])
}
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("var a={children:(b=O,'g'===b.tag?b.children:[b])};", code);
        }

        [Fact]
        public void ToJavascriptTest13()
        {
            var parser = new JavaScriptParser(@"
if (e.IsWebService)
	if (h = e.HttpRequest.responseXML, 'undefined' == typeof h) Trace.Write('Error: ' + e.UniqueId + ' data has no properties!'), m = !0;
	else try {
		h.setProperty('SelectionLanguage', 'XPath')
	} catch (l) {
		Trace.Write('Error: data.setProperty('SelectionLanguage', 'XPath') because ' + l.message)
	} else h = e.HttpRequest.responseText;
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("if(e.IsWebService)if(h=e.HttpRequest.responseXML,'undefined'==(typeof (h)))Trace.Write((('Error: '+e.UniqueId)+' data has no properties!')),m=(!0); else try {h.setProperty('SelectionLanguage','XPath');} catch(l){Trace.Write('Error: data.setProperty(',SelectionLanguage,', ',XPath,') because '+l.message);} else h=e.HttpRequest.responseText;", code);
        }

        [Fact]
        public void ToJavascriptTest14()
        {
            var source = @"function tt(t, r) {
  var n, e, i = b(t),
      s = b(r);
  if (s && (e = ft(r)), i);
  else if (s) return D(t, e) ? void $(t, e) : (n = l(e, t), G(t, n), void ht(t));
  var g, o, f;
  for (f = t.length < r.length ? t.length : r.length, o = 0, g = 0; f > g; g++) o += t[g] + r[g], t[g] = o & _t, o >>= at;
  for (g = f; o && g < t.length; g++) o += t[g], t[g] = o & _t, o >>= at
}
";
            source = Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program, true);

            var expected = @"function tt(t,r) {
    var n,e,i = (b(t)),s = (b(r));
    if (s && (e = (ft(r))), i) 
        ;
     else if (s) 
        return D(t,e)?(void ($(t,e))):(n = (l(e,t)), G(t,n), void (ht(t)));
    var g,o,f;
    for(f = (t.length < r.length?t.length:r.length), o = 0, g = 0;f > g;g++) 
        o += (t[g] + r[g]), t[g] = (o & _t), o >>= at;
    for(g = f;o && (g < t.length);g++) 
        o += t[g], t[g] = (o & _t), o >>= at;
}";
            expected = Regex.Replace(expected, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            Assert.Equal(expected, code);
        }

        [Fact]
        public void ToJavascriptTest15()
        {
            var parser = new JavaScriptParser(@"
h='M'+(+new Date).toString(36)
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("h=('M'+((+(new Date)).toString(36)));", code);
        }

        [Fact]
        public void ToJavascriptTest16()
        {
            var parser = new JavaScriptParser(@"
input.onchange = async (e) => {
            const files = await readFiles(input.files, readMode);
            document.body.removeChild(input);
            resolve(files);
        };
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("input.onchange=(async e=>{const files=await readFiles(input.files,readMode);document.body.removeChild(input);resolve(files);});", code);
        }

        [Fact]
        public void ToJavascriptTest17()
        {
            var parser = new JavaScriptParser(@"
export const Base = LegacyElementMixin(HTMLElement).prototype;
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("export const Base=(LegacyElementMixin(HTMLElement)).prototype;", code);
        }

        [Fact]
        public void ToJavascriptTest18()
        {
            var parser = new JavaScriptParser(@"
let {is} = getIsExtends(element);
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("let {is}=(getIsExtends(element));", code);
        }

        [Fact]
        public void ToJavascriptTest19()
        {
            var parser = new JavaScriptParser(@"
export const wrap =
  (window['ShadyDOM'] && window['ShadyDOM']['wrap']) || (node => node);
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("export const wrap=((window['ShadyDOM']&&window['ShadyDOM']['wrap'])||(node=>node));", code);
        }

        [Fact]
        public void ToJavascriptTest20()
        {
            var parser = new JavaScriptParser(@"
export {}");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("export{};", code);
        }

        [Fact]
        public void ToJavascriptTest21()
        {
            var parser = new JavaScriptParser(@"
(() => {
  mutablePropertyChange = MutableData._mutablePropertyChange;
})();
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("(()=>{mutablePropertyChange=MutableData._mutablePropertyChange;})();", code);
        }

        [Fact]
        public void ToJavascriptTest22()
        {
            var parser = new JavaScriptParser(@"
var Ol, jl = new (function() {
        var l, h, z;
        return l = c
    }())
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("var Ol,jl=(new((function(){var l,h,z;return l=c;})()));", code);
        }

        [Fact]
        public void ToJavascriptTest23()
        {
            var parser = new JavaScriptParser(@"

[y, {
                    [Symbol.iterator]() {
                        return b
                    },a:5
                }]
        
");
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program);
            Assert.Equal("[y,{[Symbol.iterator]:(function(){return b;}),a:5}];", code);
        }

        [Fact]
        public void ToJavascriptTest24()
        {
            var source = @"

class A { 
*[Symbol.iterator]() {
                let L = this._first;
                for (; L !== _.Undefined; )
                    yield L.element,
                    L = L.next
            }
}
        
";
            source = Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program, true);

            var expected = @"class A {
    *[Symbol.iterator]() {
        let L = this._first;
        for(;L !== _.Undefined;) 
            yield L.element, L = L.next;
    }
}";
            expected = Regex.Replace(expected, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            Assert.Equal(expected, code);
        }

        [Fact]
        public void ToJavascriptTest25()
        {
            var source = @"var i = function e(i) {
            var r = n[i];
            if (void 0 !== r)
                return r.exports;
            var a = n[i] = {
                exports: {}
            };
            return t[i](a, a.exports, e),
            a.exports
        }(15);     
";
            source = Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program, true);

            var expected = @"var i = ((function e(i) {
    var r = n[i];
    if ((void 0) !== r) 
        return r.exports;
    var a = (n[i] = {
        exports : {}
    });
    return t[i](a,a.exports,e), a.exports;
})(15));";
            expected = Regex.Replace(expected, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            Assert.Equal(expected, code);
        }

        [Fact]
        public void ToJavascriptTest26()
        {
            var source = @"class A {
    aa() {
        let a = 1;
    }
}
var b = 1;
var c;
if (b == 2) {
    c = 1;
} else {
    c = 3;
}";
            source = Regex.Replace(source, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            var parser = new JavaScriptParser(source);
            var program = parser.ParseScript();
            var code = ToJavascriptConverter.ToJavascript(program, true);
            Assert.Equal(source, code);
        }
    }
}
