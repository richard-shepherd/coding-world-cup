from __future__ import division
from position import Position
import json
#
# def obj_dict(obj):
#     for i, e in enumerate(obj):
#         return e
#     # return obj.__dict__


class JSObject(dict):
    def __init__(self):
        super(JSObject, self).__init__()
        self._dict = {}

    def add(self, key, value):
        if isinstance(value, float):
            value = round(value, 4)
            self._dict[key] = value
        elif isinstance(value, list):
            new_list = []
            for item in value:
                if isinstance(item, JSObject):
                    new_list.append(item._dict)
                else:
                    raise TypeError("Unexpected instance type %s" % item.__class__.__name__)

            self._dict[key] = new_list

        elif isinstance(value, Position):
            new_dict = {"x": value.x, "y": value.y}
            self._dict[key] = new_dict

        else:
            self._dict[key] = value

    def get_value(self, key):
        return self._dict[key]

    # Converts the object to a JSON string.
    def to_json(self):
        return json.dumps(self._dict, separators=(',', ':'))

    def jprint(self):
        print(json.dumps(self._dict, separators=(',', ':')))

    @property
    def dict(self):
        return self._dict



