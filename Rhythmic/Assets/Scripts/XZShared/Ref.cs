using System;

using static Logging;

namespace XZShared {
    public class Ref {
        public Ref() { }
        public Ref(Func<object> getter, Action<object> setter) {
            this.getter = getter;
            this.setter = setter;
            var_type = get_value().GetType(); // HACK: 
        }
        public Type  var_type;
        public Func  <object> getter;
        public Action<object> setter;
        public object get_value()             => getter.Invoke();
        public void   set_value(object value) => setter.Invoke(value);

        // Nudges the value in a certain direction.
        // In case of booleans, it will simply toggle the value, no matter the 'nudge_by'.
        public bool nudge_value(double nudge_by = 1, bool rollover_enums = true) {
            // We cannot easily use a switch here, as Types are not constant expressions in C#.
            // There are ways to use a switch, but it doesn't look as clean.
            if      (var_type == typeof(int))    set_value((int)   get_value() + (int)nudge_by);
            else if (var_type == typeof(float))  set_value((float) get_value() + (float)nudge_by);
            else if (var_type == typeof(double)) set_value((double)get_value() + nudge_by);
            else if (var_type == typeof(bool))   set_value(!(bool) get_value());
            else if (var_type.BaseType == typeof(Enum)) {
                int count  = Enum.GetValues(var_type).Length;
                int target = (int)get_value() + (int)nudge_by;
                if (rollover_enums) {
                    if      (target >= count) target = 0;
                    else if (target < 0)      target = count - 1;
                } else if (target >= count || target < 0) {
                    log_warning("index under or overflow for type '%'! ignoring".interp(var_type.Name));
                    return false;
                }
                object result = Enum.Parse(var_type, target.ToString());
                set_value(result);
            } else {
                log_warning("unsupported type: '%'".interp(var_type.Name));
                return false;
            }

            return true;
        }
    }

    public class Ref<T> : Ref {
        public Ref(Func<T> getter, Action<T> setter) {
            this.getter = ( ) => getter.Invoke();
            this.setter = (v) => setter.Invoke((T)v);
            getter_typed = getter;
            setter_typed = setter;
            var_type = typeof(T);
        }
        public Func  <T> getter_typed;
        public Action<T> setter_typed;
        public T    get_value_typed()        => getter_typed.Invoke();
        public void set_value_typed(T value) => setter_typed.Invoke(value);
    }
}