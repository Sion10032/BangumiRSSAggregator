import React from "react";

export function cloneAndUpdateProperty<TObject extends object, TPropertyName extends keyof TObject>(obj : TObject, propertyName: TPropertyName, newValue : TObject[TPropertyName]) {
  const newObj = { ...obj };
  newObj[propertyName] = newValue;
  return newObj;
}

export function getSelectedItems<T>(objs : T[], keys : React.Key[], keySelector : (obj : T) => React.Key) {
  return objs.filter(o => keys.findIndex(k =>  k === keySelector(o)) != -1);
}

export function getSelectedItem<T>(objs : T[], key : React.Key, keySelector : (obj : T) => React.Key) {
  return objs.find(o => keySelector(o) === key);
}
