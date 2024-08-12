import { Dispatch, SetStateAction } from "react";

// export type Func<TIn, TOut> = (param : TIn) => TOut;
// export type AsyncFunc<TIn, TOut> = Func<TIn, Promise<TOut>>;

// export type Action<TIn> = Func<TIn, void>;
// export type AsyncAction<TIn> = AsyncFunc<TIn, void>;

export type State<S> = [S, Dispatch<SetStateAction<S>>]; 

export type FormModalDefaultProps<T extends object> = {
  isOpen : boolean;
  defaultValue? : T;
  onConfirm : (value : T) => Promise<void>;
  onCancel : () => Promise<void>;
};

export type Dict<TKey extends keyof any, TValue> = {
  [ key in TKey ] : TValue;
};
