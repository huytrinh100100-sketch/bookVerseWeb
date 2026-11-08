export type Nullable<T> = T | null;

/** Trang thai chung dung chung cho moi module (khop BE enum). */
export type ActiveState = 'active' | 'inactive';

export interface TreeNode {
  id: string;
  code: string;
  name: string;
  parentId?: Nullable<string>;
  sortOrder: number;
  isActive: boolean;
  children: TreeNode[];
}

export interface OptionItem {
  label: string;
  value: string;
}

export interface KeyValuePair {
  key: string;
  value: string;
}
