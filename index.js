{
  // set console size and title
  child_process.execSync('MODE CON: COLS=56 LINES=32');

  let {Math, Object, Set, console} = global;
  // reserved global constants/variables
  let globalReserved = new Set(["Infinity", "NaN", "undefined", "global", "_", "_error"]);
  for(let name of Object.getOwnPropertyNames(global)) {
    if(!globalReserved.has(name) && Object.getOwnPropertyDescriptor(global, name).configurable) {
      delete global[name];
    }
  }
  // reserved prototype properties
  delete global.__proto__.constructor;
  let protoReserved = new Set(['toString']);
  let proto = Object.prototype;
  for(let name of Object.getOwnPropertyNames(proto)) {
    if(!protoReserved.has(name)) {
      delete proto[name];
    }
  }
  // expose Math functions/constants
  let MathExposed = Object.getOwnPropertyNames(Math);
  for(let name of MathExposed) {
    let desc = Object.getOwnPropertyDescriptor(Math, name);
    if(desc.writable) {
      desc.writable = false;
    }
    Object.defineProperty(global, name, desc);
  }
  let consoleExposed = ['assert', 'clear'];
  for(let name of consoleExposed) {
    Object.defineProperty(global, name, Object.getOwnPropertyDescriptor(console, name));
  }
}
