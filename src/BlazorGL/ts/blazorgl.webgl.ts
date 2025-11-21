type ContextInfo = {
  gl: WebGL2RenderingContext | WebGLRenderingContext;
  vaoExt?: OES_vertex_array_object;
  instancingExt?: ANGLE_instanced_arrays;
  consts: Record<string, number>;
};

const contexts = new Map<number, ContextInfo>();
const buffers = new Map<number, WebGLBuffer>();
const textures = new Map<number, WebGLTexture>();
const framebuffers = new Map<number, WebGLFramebuffer>();
const renderbuffers = new Map<number, WebGLRenderbuffer>();
const shaders = new Map<number, WebGLShader>();
const programs = new Map<number, WebGLProgram>();
const vaos = new Map<number, WebGLVertexArrayObject>();

let ctxSeq = 1;
let handleSeq = 1;

function makeConstTable(gl: WebGLRenderingContext | WebGL2RenderingContext): Record<string, number> {
  // Map the symbolic names used in C# to WebGL constants.
  return {
    DepthTest: gl.DEPTH_TEST,
    CullFace: gl.CULL_FACE,
    Blend: gl.BLEND,
    Back: gl.BACK,
    Front: gl.FRONT,
    FrontAndBack: gl.FRONT_AND_BACK,
    Ccw: gl.CCW,
    SrcAlpha: gl.SRC_ALPHA,
    OneMinusSrcAlpha: gl.ONE_MINUS_SRC_ALPHA,
    ColorBufferBit: gl.COLOR_BUFFER_BIT,
    DepthBufferBit: gl.DEPTH_BUFFER_BIT,
    StencilBufferBit: gl.STENCIL_BUFFER_BIT,
    ArrayBuffer: gl.ARRAY_BUFFER,
    ElementArrayBuffer: gl.ELEMENT_ARRAY_BUFFER,
    StaticDraw: gl.STATIC_DRAW,
    DynamicDraw: gl.DYNAMIC_DRAW,
    Texture2D: gl.TEXTURE_2D,
    TextureWrapS: gl.TEXTURE_WRAP_S,
    TextureWrapT: gl.TEXTURE_WRAP_T,
    TextureMinFilter: gl.TEXTURE_MIN_FILTER,
    TextureMagFilter: gl.TEXTURE_MAG_FILTER,
    Repeat: gl.REPEAT,
    ClampToEdge: gl.CLAMP_TO_EDGE,
    MirroredRepeat: gl.MIRRORED_REPEAT,
    Linear: gl.LINEAR,
    Nearest: gl.NEAREST,
    NearestMipmapNearest: gl.NEAREST_MIPMAP_NEAREST,
    LinearMipmapNearest: gl.LINEAR_MIPMAP_NEAREST,
    NearestMipmapLinear: gl.NEAREST_MIPMAP_LINEAR,
    LinearMipmapLinear: gl.LINEAR_MIPMAP_LINEAR,
    Framebuffer: gl.FRAMEBUFFER,
    Renderbuffer: gl.RENDERBUFFER,
    ColorAttachment0: gl.COLOR_ATTACHMENT0,
    DepthAttachment: gl.DEPTH_ATTACHMENT,
    Rgba: gl.RGBA,
    DepthComponent16: gl.DEPTH_COMPONENT16,
    UnsignedByte: gl.UNSIGNED_BYTE,
    Texture0: gl.TEXTURE0,
    Triangles: gl.TRIANGLES,
    LineStrip: gl.LINE_STRIP,
    Lines: gl.LINES,
    LineLoop: gl.LINE_LOOP,
    Points: gl.POINTS,
    UnsignedInt: (gl as WebGL2RenderingContext).UNSIGNED_INT ?? gl.UNSIGNED_SHORT,
    VertexShader: gl.VERTEX_SHADER,
    FragmentShader: gl.FRAGMENT_SHADER,
    ActiveUniforms: gl.ACTIVE_UNIFORMS,
    ActiveAttributes: gl.ACTIVE_ATTRIBUTES,
    Float: gl.FLOAT,
    FramebufferComplete: gl.FRAMEBUFFER_COMPLETE
  };
}

function getContext(id: number): ContextInfo {
  const ctx = contexts.get(id);
  if (!ctx) throw new Error(`WebGL context ${id} not found`);
  return ctx;
}

function constOf(ctx: ContextInfo, name: string): number {
  const value = ctx.consts[name];
  if (value === undefined) throw new Error(`Unknown GL constant '${name}'`);
  return value;
}

export function createContext(canvas: HTMLCanvasElement): number {
  const gl = (canvas.getContext("webgl2") ??
    canvas.getContext("webgl")) as WebGL2RenderingContext | WebGLRenderingContext | null;

  if (!gl) throw new Error("Unable to create WebGL context.");

  const vaoExt = "createVertexArray" in gl ? undefined : (gl as any).getExtension("OES_vertex_array_object");
  const instancingExt = "drawElementsInstanced" in gl ? undefined : (gl as any).getExtension("ANGLE_instanced_arrays");

  const id = ctxSeq++;
  contexts.set(id, { gl, vaoExt, instancingExt, consts: makeConstTable(gl) });
  return id;
}

export function enable(ctxId: number, cap: string) {
  const ctx = getContext(ctxId);
  ctx.gl.enable(constOf(ctx, cap));
}
export function disable(ctxId: number, cap: string) {
  const ctx = getContext(ctxId);
  ctx.gl.disable(constOf(ctx, cap));
}
export function cullFace(ctxId: number, mode: string) {
  const ctx = getContext(ctxId);
  ctx.gl.cullFace(constOf(ctx, mode));
}
export function frontFace(ctxId: number, dir: string) {
  const ctx = getContext(ctxId);
  ctx.gl.frontFace(constOf(ctx, dir));
}
export function blendFunc(ctxId: number, src: string, dst: string) {
  const ctx = getContext(ctxId);
  ctx.gl.blendFunc(constOf(ctx, src), constOf(ctx, dst));
}
export function viewport(ctxId: number, x: number, y: number, w: number, h: number) {
  const ctx = getContext(ctxId);
  ctx.gl.viewport(x, y, w, h);
}
export function clearColor(ctxId: number, r: number, g: number, b: number, a: number) {
  const ctx = getContext(ctxId);
  ctx.gl.clearColor(r, g, b, a);
}
export function clear(ctxId: number, mask: string) {
  const ctx = getContext(ctxId);
  ctx.gl.clear(constOf(ctx, mask));
}

export function clearMultiple(ctxId: number, masks: string[]) {
  const ctx = getContext(ctxId);
  const maskValue = masks.reduce((acc, name) => acc | constOf(ctx, name), 0);
  ctx.gl.clear(maskValue);
}

export function createVertexArray(ctxId: number): number {
  const ctx = getContext(ctxId);
  const vao = ("createVertexArray" in ctx.gl
    ? (ctx.gl as WebGL2RenderingContext).createVertexArray()
    : ctx.vaoExt?.createVertexArrayOES()) as WebGLVertexArrayObject | null;
  const id = handleSeq++;
  if (vao) vaos.set(id, vao);
  return vao ? id : 0;
}

export function bindVertexArray(ctxId: number, vaoId: number) {
  const ctx = getContext(ctxId);
  const vao = vaoId === 0 ? null : vaos.get(vaoId) ?? null;
  if ("bindVertexArray" in ctx.gl) {
    (ctx.gl as WebGL2RenderingContext).bindVertexArray(vao);
  } else {
    ctx.vaoExt?.bindVertexArrayOES(vao);
  }
}

export function createBuffer(ctxId: number): number {
  const ctx = getContext(ctxId);
  const buffer = ctx.gl.createBuffer();
  const id = handleSeq++;
  if (buffer) buffers.set(id, buffer);
  return buffer ? id : 0;
}

export function bindBuffer(ctxId: number, target: string, bufferId: number) {
  const ctx = getContext(ctxId);
  const targetEnum = constOf(ctx, target);
  const buffer = bufferId === 0 ? null : buffers.get(bufferId) ?? null;
  ctx.gl.bindBuffer(targetEnum, buffer);
}

export function bufferDataFloat(ctxId: number, target: string, data: number[], usage: string) {
  const ctx = getContext(ctxId);
  ctx.gl.bufferData(constOf(ctx, target), new Float32Array(data), constOf(ctx, usage));
}

export function bufferDataUInt(ctxId: number, target: string, data: number[], usage: string) {
  const ctx = getContext(ctxId);
  ctx.gl.bufferData(constOf(ctx, target), new Uint32Array(data), constOf(ctx, usage));
}

export function createTexture(ctxId: number): number {
  const ctx = getContext(ctxId);
  const texture = ctx.gl.createTexture();
  const id = handleSeq++;
  if (texture) textures.set(id, texture);
  return texture ? id : 0;
}

export function bindTexture(ctxId: number, target: string, textureId: number) {
  const ctx = getContext(ctxId);
  const texture = textureId === 0 ? null : textures.get(textureId) ?? null;
  ctx.gl.bindTexture(constOf(ctx, target), texture);
}

export function texImage2D(
  ctxId: number,
  target: string,
  level: number,
  internalFormat: string,
  width: number,
  height: number,
  format: string,
  type: string,
  data: number[] | null
) {
  const ctx = getContext(ctxId);
  const gl = ctx.gl;
  const targetEnum = constOf(ctx, target);
  const internal = constOf(ctx, internalFormat);
  const fmt = constOf(ctx, format);
  const typ = constOf(ctx, type);
  const array = data ? new Uint8Array(data) : null;
  gl.texImage2D(targetEnum, level, internal, width, height, 0, fmt, typ, array);
}

export function texParameter(ctxId: number, target: string, pname: string, value: string) {
  const ctx = getContext(ctxId);
  ctx.gl.texParameteri(constOf(ctx, target), constOf(ctx, pname), constOf(ctx, value));
}

export function generateMipmap(ctxId: number, target: string) {
  const ctx = getContext(ctxId);
  ctx.gl.generateMipmap(constOf(ctx, target));
}

export function createFramebuffer(ctxId: number): number {
  const ctx = getContext(ctxId);
  const fb = ctx.gl.createFramebuffer();
  const id = handleSeq++;
  if (fb) framebuffers.set(id, fb);
  return fb ? id : 0;
}

export function bindFramebuffer(ctxId: number, target: string, fbId: number) {
  const ctx = getContext(ctxId);
  const fb = fbId === 0 ? null : framebuffers.get(fbId) ?? null;
  ctx.gl.bindFramebuffer(constOf(ctx, target), fb);
}

export function framebufferTexture2D(
  ctxId: number,
  target: string,
  attachment: string,
  texTarget: string,
  texId: number,
  level: number
) {
  const ctx = getContext(ctxId);
  ctx.gl.framebufferTexture2D(
    constOf(ctx, target),
    constOf(ctx, attachment),
    constOf(ctx, texTarget),
    textures.get(texId) ?? null,
    level
  );
}

export function createRenderbuffer(ctxId: number): number {
  const ctx = getContext(ctxId);
  const rb = ctx.gl.createRenderbuffer();
  const id = handleSeq++;
  if (rb) renderbuffers.set(id, rb);
  return rb ? id : 0;
}

export function bindRenderbuffer(ctxId: number, target: string, rbId: number) {
  const ctx = getContext(ctxId);
  const rb = rbId === 0 ? null : renderbuffers.get(rbId) ?? null;
  ctx.gl.bindRenderbuffer(constOf(ctx, target), rb);
}

export function renderbufferStorage(ctxId: number, target: string, format: string, width: number, height: number) {
  const ctx = getContext(ctxId);
  ctx.gl.renderbufferStorage(constOf(ctx, target), constOf(ctx, format), width, height);
}

export function framebufferRenderbuffer(
  ctxId: number,
  target: string,
  attachment: string,
  renderbufferTarget: string,
  rbId: number
) {
  const ctx = getContext(ctxId);
  ctx.gl.framebufferRenderbuffer(
    constOf(ctx, target),
    constOf(ctx, attachment),
    constOf(ctx, renderbufferTarget),
    renderbuffers.get(rbId) ?? null
  );
}

export function checkFramebufferStatus(ctxId: number, target: string): number {
  const ctx = getContext(ctxId);
  return ctx.gl.checkFramebufferStatus(constOf(ctx, target));
}

export function deleteTexture(ctxId: number, texId: number) {
  const ctx = getContext(ctxId);
  const tex = textures.get(texId);
  if (tex) ctx.gl.deleteTexture(tex);
  textures.delete(texId);
}

export function deleteFramebuffer(ctxId: number, fbId: number) {
  const ctx = getContext(ctxId);
  const fb = framebuffers.get(fbId);
  if (fb) ctx.gl.deleteFramebuffer(fb);
  framebuffers.delete(fbId);
}

export function deleteRenderbuffer(ctxId: number, rbId: number) {
  const ctx = getContext(ctxId);
  const rb = renderbuffers.get(rbId);
  if (rb) ctx.gl.deleteRenderbuffer(rb);
  renderbuffers.delete(rbId);
}

export function deleteBuffer(ctxId: number, bufferId: number) {
  const ctx = getContext(ctxId);
  const buffer = buffers.get(bufferId);
  if (buffer) ctx.gl.deleteBuffer(buffer);
  buffers.delete(bufferId);
}

export function deleteVertexArray(ctxId: number, vaoId: number) {
  const ctx = getContext(ctxId);
  const vao = vaos.get(vaoId);
  if ("deleteVertexArray" in ctx.gl) {
    (ctx.gl as WebGL2RenderingContext).deleteVertexArray(vao ?? null);
  } else {
    ctx.vaoExt?.deleteVertexArrayOES(vao ?? null);
  }
  vaos.delete(vaoId);
}

export function activeTexture(ctxId: number, unitIndex: number) {
  const ctx = getContext(ctxId);
  const base = constOf(ctx, "Texture0");
  ctx.gl.activeTexture(base + unitIndex);
}

export function drawElements(
  ctxId: number,
  mode: string,
  count: number,
  type: string,
  offset: number
) {
  const ctx = getContext(ctxId);
  ctx.gl.drawElements(constOf(ctx, mode), count, constOf(ctx, type), offset);
}

export function drawElementsInstanced(
  ctxId: number,
  mode: string,
  count: number,
  type: string,
  offset: number,
  instanceCount: number
) {
  const ctx = getContext(ctxId);
  const gl = ctx.gl as WebGL2RenderingContext;
  if ("drawElementsInstanced" in gl) {
    gl.drawElementsInstanced(constOf(ctx, mode), count, constOf(ctx, type), offset, instanceCount);
  } else if (ctx.instancingExt) {
    ctx.instancingExt.drawElementsInstancedANGLE(constOf(ctx, mode), count, constOf(ctx, type), offset, instanceCount);
  } else {
    throw new Error("Instanced rendering not supported in this context.");
  }
}

export function drawArrays(ctxId: number, mode: string, first: number, count: number) {
  const ctx = getContext(ctxId);
  ctx.gl.drawArrays(constOf(ctx, mode), first, count);
}

export function vertexAttribPointer(
  ctxId: number,
  index: number,
  size: number,
  type: string,
  normalized: boolean,
  stride: number,
  offset: number
) {
  const ctx = getContext(ctxId);
  ctx.gl.vertexAttribPointer(index, size, constOf(ctx, type), normalized, stride, offset);
}

export function vertexAttribDivisor(ctxId: number, index: number, divisor: number) {
  const ctx = getContext(ctxId);
  const gl = ctx.gl as WebGL2RenderingContext;
  if ("vertexAttribDivisor" in gl) {
    gl.vertexAttribDivisor(index, divisor);
  } else if (ctx.instancingExt) {
    ctx.instancingExt.vertexAttribDivisorANGLE(index, divisor);
  }
}

export function enableVertexAttribArray(ctxId: number, index: number) {
  const ctx = getContext(ctxId);
  ctx.gl.enableVertexAttribArray(index);
}

export function createShader(ctxId: number, type: string): number {
  const ctx = getContext(ctxId);
  const shader = ctx.gl.createShader(constOf(ctx, type));
  const id = handleSeq++;
  if (shader) shaders.set(id, shader);
  return shader ? id : 0;
}

export function shaderSource(ctxId: number, shaderId: number, source: string) {
  const ctx = getContext(ctxId);
  ctx.gl.shaderSource(shaders.get(shaderId)!, source);
}

export function compileShader(ctxId: number, shaderId: number) {
  const ctx = getContext(ctxId);
  ctx.gl.compileShader(shaders.get(shaderId)!);
}

export function getShaderInfoLog(ctxId: number, shaderId: number): string {
  const ctx = getContext(ctxId);
  return ctx.gl.getShaderInfoLog(shaders.get(shaderId)!) ?? "";
}

export function createProgram(ctxId: number): number {
  const ctx = getContext(ctxId);
  const program = ctx.gl.createProgram();
  const id = handleSeq++;
  if (program) programs.set(id, program);
  return program ? id : 0;
}

export function attachShader(ctxId: number, programId: number, shaderId: number) {
  const ctx = getContext(ctxId);
  ctx.gl.attachShader(programs.get(programId)!, shaders.get(shaderId)!);
}

export function linkProgram(ctxId: number, programId: number) {
  const ctx = getContext(ctxId);
  ctx.gl.linkProgram(programs.get(programId)!);
}

export function getProgramInfoLog(ctxId: number, programId: number): string {
  const ctx = getContext(ctxId);
  return ctx.gl.getProgramInfoLog(programs.get(programId)!) ?? "";
}

export function deleteShader(ctxId: number, shaderId: number) {
  const ctx = getContext(ctxId);
  const shader = shaders.get(shaderId);
  if (shader) ctx.gl.deleteShader(shader);
  shaders.delete(shaderId);
}

export function useProgram(ctxId: number, programId: number) {
  const ctx = getContext(ctxId);
  ctx.gl.useProgram(programs.get(programId) ?? null);
}

export function getProgramParameter(ctxId: number, programId: number, property: string): number {
  const ctx = getContext(ctxId);
  return ctx.gl.getProgramParameter(programs.get(programId)!, constOf(ctx, property)) as number;
}

export function getActiveUniform(ctxId: number, programId: number, index: number) {
  const ctx = getContext(ctxId);
  const info = ctx.gl.getActiveUniform(programs.get(programId)!, index);
  return info
    ? { name: info.name, size: info.size, type: info.type }
    : { name: "", size: 0, type: 0 };
}

export function getUniformLocation(ctxId: number, programId: number, name: string): number {
  const ctx = getContext(ctxId);
  const location = ctx.gl.getUniformLocation(programs.get(programId)!, name);
  return location === null ? -1 : (location as any);
}

export function getActiveAttrib(ctxId: number, programId: number, index: number) {
  const ctx = getContext(ctxId);
  const info = ctx.gl.getActiveAttrib(programs.get(programId)!, index);
  return info
    ? { name: info.name, size: info.size, type: info.type }
    : { name: "", size: 0, type: 0 };
}

export function getAttribLocation(ctxId: number, programId: number, name: string): number {
  const ctx = getContext(ctxId);
  return ctx.gl.getAttribLocation(programs.get(programId)!, name);
}

export function uniform1i(ctxId: number, location: number, value: number) {
  const ctx = getContext(ctxId);
  ctx.gl.uniform1i(location, value);
}

export function uniform1f(ctxId: number, location: number, value: number) {
  const ctx = getContext(ctxId);
  ctx.gl.uniform1f(location, value);
}

export function uniform2f(ctxId: number, location: number, x: number, y: number) {
  const ctx = getContext(ctxId);
  ctx.gl.uniform2f(location, x, y);
}

export function uniform3f(ctxId: number, location: number, x: number, y: number, z: number) {
  const ctx = getContext(ctxId);
  ctx.gl.uniform3f(location, x, y, z);
}

export function uniform4f(ctxId: number, location: number, x: number, y: number, z: number, w: number) {
  const ctx = getContext(ctxId);
  ctx.gl.uniform4f(location, x, y, z, w);
}

export function uniformMatrix4fv(ctxId: number, location: number, transpose: boolean, values: number[]) {
  const ctx = getContext(ctxId);
  ctx.gl.uniformMatrix4fv(location, transpose, new Float32Array(values));
}

export function depthMask(ctxId: number, flag: boolean) {
  const ctx = getContext(ctxId);
  ctx.gl.depthMask(flag);
}
