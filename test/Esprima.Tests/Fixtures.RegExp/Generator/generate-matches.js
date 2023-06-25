const readline = require('readline');

function decodeStringIfEscaped(s) {
    return s.length > 2 && (s.startsWith('"') && s.endsWith('"') || s.startsWith("'") && s.endsWith("'"))
        ? eval(s)
        : s;
}

async function processInputAsync(rl) {
    for await (const line of rl) {
        try {
            let [pattern, flags, testString] = line.split('\t');

            if (typeof pattern !== "string" || typeof flags !== "string" || typeof testString !== "string") {
                throw new Error("Invalid input data: " + line);
            }

            pattern = decodeStringIfEscaped(pattern);
            flags = decodeStringIfEscaped(flags);
            testString = decodeStringIfEscaped(testString);

            let regexp, syntaxError;
            try { regexp = new RegExp(pattern, flags + 'g'); }
            catch (err) {
                let match;
                if (err instanceof SyntaxError && (match = /^Invalid regular expression: .*\/: (.*)$/.exec(err.message))) {
                    syntaxError = match[1];
                }
                else {
                    throw err;
                }
            }

            const result = syntaxError
                ?? [...testString.matchAll(regexp)].map(m => ({
                    captures: m,
                    index: m.index,
                    groups: m.groups
                }));

            const resultJson = JSON.stringify(result)
                // https://github.com/expressjs/express/issues/1132
                .replace(/[\u2028\u2029]/g, c => '\\u' + c.charCodeAt(0).toString(16));

            console.log(resultJson);
        }
        catch (err) {
            console.error(err);
            process.exit(-1);
        }
    }
}

processInputAsync(readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: false,
}));
