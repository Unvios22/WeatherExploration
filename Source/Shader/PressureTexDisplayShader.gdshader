shader_type spatial;
render_mode unshaded;

uniform sampler2D pressureTex;

uniform sampler2D coloringGradient : source_color;

void fragment() {
	float pressureVal = texture(pressureTex, UV).r;
	ALBEDO = texture(coloringGradient, vec2(pressureVal, 0.5)).xyz;
}