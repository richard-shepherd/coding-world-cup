from __future__ import division
import math
from math import atan2, degrees, pi
import logging


class Vector:
    # Represents a vector between two Positions.
    def __init__(self, x=0.0, y=0.0, pos_from=None, pos_to=None):
        if pos_from is None:
            self.x = x
            self.y = y
        else:
            self.x = pos_to.x - pos_from.x
            self.y = pos_to.y - pos_from.y

    # The length of the vector.
    @property
    def length(self):
        x_squared = self.x * self.x
        y_squared = self.y * self.y
        return round(math.sqrt(x_squared + y_squared), 2)

    @property
    def direction(self):
        rads = atan2(self.x, -self.y)
        rads %= 2*pi
        degs = degrees(rads)
        return round(degs, 2)

    # Returns a new Vector, scaled to the length passed in.
    def get_scaled_vector(self, length):
        if self.length == 0.0:
            return Vector(0.0, 0.0)
        else:
            x = self.x * length / self.length
            y = self.y * length / self.length
            return Vector(x, y)

    def get_rotated_vector(self, deg):
        cos = math.cos(math.radians(deg))
        sin = math.sin(math.radians(deg))

        new_x = self.x*cos - self.y*sin
        new_y = self.y*cos + self.x*sin

        return Vector(x=new_x, y=new_y)

    def get_opposite_vector(self):
        return Vector(x=-self.x, y=-self.y)

    def angle_from(self, vector=None, direction=-1):
        # Players turn at 600 degrees per second
        if isinstance(vector, Vector):
            deg_diff = abs(vector.direction - self.direction)
        elif direction >= 0:
            deg_diff = abs(direction - self.direction)
        else:
            raise ValueError("Didn't input vector or direction in angle_from method")

        if deg_diff > 180:
            deg_diff = 360 - deg_diff
            # logging.debug("the deg_diff was bigger than 180, changing to [%d]"%(deg_diff))

        return deg_diff

    def turning_time(self, vector=None, direction=-1):
        # Players turn at 600 degrees per second
        return math.ceil(self.angle_from(vector=vector, direction=direction) / 600 * 100000) / 100000



