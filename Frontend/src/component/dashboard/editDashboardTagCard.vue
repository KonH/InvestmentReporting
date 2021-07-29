<template>
	<span>
		<span class="form-group">
			<label class="mr-2">
				Tag:
				<select ref="tag" class="form-control">
					<option v-for="tag in tags" :key="tag" :value="tag">{{ tag }}</option>
				</select>
			</label>
			<label class="mr-2">
				Target:
				<input ref="target" type="number" class="form-control" value="0" />
			</label>
		</span>
		<button class="btn btn-sm btn-primary mr-1" @click="add">+</button>
		<button class="btn btn-sm btn-outline-primary" @click="close">-</button>
	</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop, Ref } from 'vue-property-decorator';
import { DashboardConfigTagDto } from '@/api/meta';

@Options({
	name: 'EditDashboardTagCard',
	emits: ['edit', 'cancel'],
})
export default class EditDashboardTagCard extends Vue {
	@Prop()
	tags!: string[];

	@Prop()
	tag!: DashboardConfigTagDto;

	@Prop()
	index!: number;

	@Ref('tag')
	tagInput!: HTMLInputElement;

	@Ref('target')
	targetInput!: HTMLInputElement;

	async mounted() {
		this.tagInput.value = this.tag.tag ?? '';
		this.targetInput.value = (this.tag.target ?? 0).toString();
	}

	async add() {
		const tag: DashboardConfigTagDto = {
			tag: this.tagInput.value,
			target: Number.parseFloat(this.targetInput.value),
		};
		this.$emit('edit', this.index, tag);
		this.close();
	}

	close() {
		this.$emit('cancel');
	}
}
</script>
